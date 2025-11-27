using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core.Validation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.CompleteMultipartUpload;

public class Handler
{
    private readonly IValidator<CompleteMultipartUploadRequest> _validator;
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<Handler> _logger;

    public Handler(
        IValidator<CompleteMultipartUploadRequest> validator,
        IMediaRepository mediaRepository,
        IS3Provider s3Provider, ILogger<Handler> logger)
    {
        _validator = validator;
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<CompleteMultipartUploadResponse, Errors>> HandleAsync(
        CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        var assetResult = await _mediaRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();
        MediaAsset asset = assetResult.Value;
        
        if (request.PartETags.Count != asset.MediaData.ExpectedChunksCount)
            return Error.Failure(
                $"part.etags.error", 
                $"PartETags does not match expected chunk count {asset.MediaData.ExpectedChunksCount}")
                .ToErrors();

        var s3CompleteUploadResult =
            await _s3Provider.CompleteMultipartUploadAsync(
                asset.RawKey,
                request.UploadId,
                request.PartETags,
                cancellationToken);
        if (s3CompleteUploadResult.IsFailure)
            return s3CompleteUploadResult.Error.ToErrors();

        var markUploadedResult = asset.MarkUploaded(DateTime.UtcNow);
        if (markUploadedResult.IsFailure)
            return markUploadedResult.Error.ToErrors();

        var saveChangesResult = await _mediaRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        _logger.LogInformation("MultipartUpload completed successfully for MediaAsset={MediaAssetId}", asset.Id);
        return new CompleteMultipartUploadResponse
        {
            MediaAssetId = asset.Id
        };
    }
}