using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Application.Services;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core.Validation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.StartMultipartUpload;

public class Handler
{
    private readonly IValidator<StartMultipartUploadRequest> _validator; 
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IS3Provider _s3Provider;
    private readonly MediaOwnerFactory _mediaOwnerFactory;
    private readonly MediaAssetFactory _mediaAssetFactory;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<Handler> _logger;
    
    public Handler(
        IValidator<StartMultipartUploadRequest> validator,
        IChunkSizeCalculator chunkSizeCalculator,
        IS3Provider s3Provider,
        MediaOwnerFactory mediaOwnerFactory, 
        MediaAssetFactory mediaAssetFactory, 
        IMediaRepository mediaRepository,
        ILogger<Handler> logger)
    {
        _validator = validator;
        _s3Provider = s3Provider;
        _mediaOwnerFactory = mediaOwnerFactory;
        _mediaAssetFactory = mediaAssetFactory;
        _mediaRepository = mediaRepository;
        _chunkSizeCalculator = chunkSizeCalculator;
        _logger = logger;
    }

    public async Task<Result<StartMultipartUploadResponse, Errors>> HandleAsync(
        StartMultipartUploadRequest request, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        
        var fileNameResult = FileName.Create(request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error.ToErrors();

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error.ToErrors();
        
        var (totalChunks, chunkSize) = _chunkSizeCalculator.Calculate(request.Size);
        
        var mediaDataResult = MediaData.Create(fileNameResult.Value, contentTypeResult.Value, request.Size, totalChunks );
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error.ToErrors();

        var mediaOwnerResult = _mediaOwnerFactory.Create(request.Context, request.ContextId);
        if (mediaOwnerResult.IsFailure)
            return mediaOwnerResult.Error.ToErrors();

        var assetResult = _mediaAssetFactory.CreateForUpload(mediaDataResult.Value, mediaOwnerResult.Value);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();
        var asset = assetResult.Value;

        var uploadIdResult = await _s3Provider.StartMultipartUploadAsync(asset.RawKey, asset.MediaData, cancellationToken);
        if (uploadIdResult.IsFailure)
            return uploadIdResult.Error.ToErrors();
        
        var assetAddResult = await _mediaRepository.AddAsync(asset, cancellationToken);
        if (assetAddResult.IsFailure)
            return assetAddResult.Error.ToErrors();
        
        var generateUrlsResult = await _s3Provider.GenerateAllChunkUploadUrlsAsync(
            asset.RawKey, uploadIdResult.Value, totalChunks, cancellationToken);
        if (generateUrlsResult.IsFailure)
            return generateUrlsResult.Error.ToErrors();
        
        var saveToDbResult = await _mediaRepository.SaveChangesAsync(cancellationToken);
        if (saveToDbResult.IsFailure)
            return saveToDbResult.Error.ToErrors();

        _logger.LogInformation("MultipartUpload started for MediaAsset {MediaAssetId}", asset.Id);
        return new StartMultipartUploadResponse()
        {
            MediaAssetId = asset.Id,
            UploadId = uploadIdResult.Value,
            ChunkUrls = generateUrlsResult.Value.Select(chunkUrl => new ChunkUploadUrlDto()
            {
                PartNumber = chunkUrl.PartNumber,
                UploadId = chunkUrl.UploadId,
                Value = chunkUrl.Value
            }).ToList(),
            ChunkSize = chunkSize
        };
    }
}