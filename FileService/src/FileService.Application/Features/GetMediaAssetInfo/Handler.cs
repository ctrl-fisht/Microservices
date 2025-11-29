using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FluentValidation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.GetMediaAssetInfo;

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

    public async Task<Result<MediaAssetInfoDto, Errors>> HandleAsync(
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

        return new MediaAssetInfoDto()
        {
            Id = asset.Id,
            Status = asset.Status.ToString(),
            AssetType = asset.AssetType.ToString(),
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt,
            FileInfo = new FileInfoDto()
            {
                FileName = asset.MediaData.FileName.Full,
                ContentType = asset.MediaData.ContentType.MimeType,
                Size = asset.MediaData.Size
            },
            DownloadUrl = presignedUrl ?? null
        };
    }
}