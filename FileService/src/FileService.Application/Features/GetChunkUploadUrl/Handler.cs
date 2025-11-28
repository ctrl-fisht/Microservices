using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FluentValidation;
using Shared.Core.Validation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.GetChunkUploadUrl;

public class Handler
{
    private readonly IValidator<GetChunkUploadUrlRequest> _validator;
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    public Handler(
        IValidator<GetChunkUploadUrlRequest> validator,
        IMediaRepository mediaRepository,
        IS3Provider s3Provider)
    {
        _validator = validator;
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
    }
    public async Task<Result<GetChunkUploadUrlResponse, Errors>> HandleAsync(
        GetChunkUploadUrlRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        var assetResult = await _mediaRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();
        var asset = assetResult.Value;
        
        var newS3Url = await _s3Provider.GenerateChunkUploadUrlAsync(
            asset.RawKey, request.UploadId, request.PartNumber);
        if (newS3Url.IsFailure)
            return newS3Url.Error.ToErrors();

        return new GetChunkUploadUrlResponse
        {
            UploadUrl = newS3Url.Value,
            PartNumber = request.PartNumber
        };
    }       
}