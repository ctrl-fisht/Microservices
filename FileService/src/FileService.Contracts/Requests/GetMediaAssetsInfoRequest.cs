namespace FileService.Contracts.Requests;

public sealed record GetMediaAssetsInfoRequest
{
    public required List<Guid> MediaAssetIds { get; init; }
}