using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Contracts.Dtos;
using FileService.Domain;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.GetMediaAssetDownloadUrl;

public class Handler
{ 
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    public Handler(
        IMediaRepository mediaRepository,
        IS3Provider s3Provider)
    {
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
    }

    public async Task<Result<string, Errors>> HandleAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        var mediaAssetResult = await _mediaRepository.GetByIdAsync(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();
        var asset = mediaAssetResult.Value;

        string? presignedUrl = null;
        if (asset.Status == Status.Ready)
        {
            var urlResult = await _s3Provider.GenerateDownloadUrlAsync(asset.FinalKey!);
            if (urlResult.IsFailure)
                return urlResult.Error.ToErrors();
            presignedUrl = urlResult.Value;
        }

        return presignedUrl != null 
            ? presignedUrl 
            : Error.NotFound("no.download.url", "Cannot found download url for media asset").ToErrors();
    }
}