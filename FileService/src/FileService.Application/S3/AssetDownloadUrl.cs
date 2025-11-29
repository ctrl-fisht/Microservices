using FileService.Domain.ValueObjects;

namespace FileService.Application.S3;

public sealed record AssetDownloadUrl
{
    public required StorageKey Key { get; init; }
    public required string Url { get; init; }
}