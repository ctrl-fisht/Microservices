using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using Shared.Kernel.Errors;

namespace FileService.Communication;

public interface IFileService
{
    Task<Result<MediaAssetInfoDto, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken ct);

    Task<Result<List<MediaAssetInfoDto>, Errors>> GetMediaAssetsInfo(GetMediaAssetsInfoRequest request,
        CancellationToken ct);
}