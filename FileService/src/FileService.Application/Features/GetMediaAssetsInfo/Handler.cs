using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Contracts.Dtos;
using FileService.Domain;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.GetMediaAssetsInfo;

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

    public async Task<Result<List<MediaAssetInfoDto>, Errors>> HandleAsync(
        List<Guid> mediaAssetIds,
        CancellationToken cancellationToken)
    {
        var assets = await _mediaRepository.GetBatchAsync(mediaAssetIds, cancellationToken);
        if (assets.Count == 0)
            return Error.NotFound("assets.not.found", "Given assets not found").ToErrors();

        var notFound = mediaAssetIds.Where(id => !assets.Any(asset => asset.Id == id)).ToList();
        if (notFound.Count > 0)
            return Error.NotFound(
                "assets.not.found",
                "Could not found assets with ids: " + string.Join(", ", notFound)).ToErrors();

        var readyAssetsStorageKeys = assets
            .Where(asset => asset.Status == Status.Ready)
            .Select(asset => asset.FinalKey ?? throw new Exception("Asset with 'Ready' state doesn't have FinalKey"))
            .ToList();
        
        var downloadUrlsResult = await _s3Provider.GenerateDownloadUrlsAsync(readyAssetsStorageKeys, cancellationToken);
        if (downloadUrlsResult.IsFailure)
            return downloadUrlsResult.Error.ToErrors();
        
        return assets.Select(asset => new MediaAssetInfoDto()
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
            DownloadUrl = downloadUrlsResult.Value
                .FirstOrDefault(du => du.Key == asset.FinalKey)
                ?.Url
        }).ToList();
    }
}