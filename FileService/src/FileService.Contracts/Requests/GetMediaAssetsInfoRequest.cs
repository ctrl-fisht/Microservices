namespace FileService.Contracts.Requests;

public sealed record GetMediaAssetsInfoRequest
{
    public required IReadOnlyList<Guid> MediaAssetIds { get; init; }
}