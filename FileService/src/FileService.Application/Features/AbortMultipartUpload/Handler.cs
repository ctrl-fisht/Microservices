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

namespace FileService.Application.Features.AbortMultipartUpload;

public sealed class Handler
{
    private readonly IValidator<AbortMultipartUploadRequest> _validator;
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<Handler> _logger;

    public Handler(
        IValidator<AbortMultipartUploadRequest> validator,
        IMediaRepository mediaRepository,
        IS3Provider s3Provider, ILogger<Handler> logger)
    {
        _validator = validator;
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<AbortMultipartUploadResponse, Errors>> HandleAsync(
        AbortMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();
        
        var assetResult = await _mediaRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error.ToErrors();
        MediaAsset asset = assetResult.Value;
        
        var abortResult = await _s3Provider.AbortMultipartUploadAsync(asset.RawKey, request.UploadId, cancellationToken);
        if (abortResult.IsFailure)
            return abortResult.Error.ToErrors();

        var markFailedResult = asset.MarkFailed(DateTime.UtcNow);
        if (markFailedResult.IsFailure)
            return markFailedResult.Error.ToErrors();
        
        var saveChangesResult = await _mediaRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        _logger.LogInformation("MultipartUpload aborted for MediaAsset={MediaAssetId}", asset.Id);
        return new AbortMultipartUploadResponse()
        {
            Success = true
        };
    }
}