using CSharpFunctionalExtensions;
using FileService.Domain.Entities;
using Shared.Kernel.Errors;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<VideoAsset, Error> CreateVideoForUpload(MediaData mediaData, MediaOwner mediaOwner);
    Result<PreviewAsset, Error> CreatePreviewForUpload(MediaData mediaData, MediaOwner mediaOwner);
}