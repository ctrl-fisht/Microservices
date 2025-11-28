using CSharpFunctionalExtensions;
using FileService.Domain.Entities;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Application.Services;

public class MediaAssetFactory
{
    public Result<MediaAsset, Error> CreateForUpload(MediaData mediaData, MediaOwner owner)
    {
        return mediaData.ContentType.MediaType switch
        {
            MediaType.Video => CreateVideoAsset(mediaData, owner),
            MediaType.Image => CreatePreviewAsset(mediaData, owner),
            _ => Error.Failure("unknown.media.type", "Cannot create media asset, unknown media type")
        };
    }

    private VideoAsset CreateVideoAsset(MediaData mediaData, MediaOwner owner)
    {
        var key = Guid.NewGuid().ToString();
        var rawKey= StorageKey.Create(VideoAsset.BUCKET, key, VideoAsset.RAW_PREFIX).Value;
        var hlsRootKey = StorageKey.Create(VideoAsset.BUCKET, key, VideoAsset.HLS_PREFIX).Value;
        
        return VideoAsset.Create(mediaData, rawKey, owner, hlsRootKey).Value;
    }

    private PreviewAsset CreatePreviewAsset(MediaData mediaData, MediaOwner owner)
    {
        var key = Guid.NewGuid().ToString();
        var storageKey = StorageKey.Create(PreviewAsset.BUCKET, key, PreviewAsset.RAW_PREFIX).Value;
        return PreviewAsset.Create(mediaData, storageKey, owner).Value;
    }
}