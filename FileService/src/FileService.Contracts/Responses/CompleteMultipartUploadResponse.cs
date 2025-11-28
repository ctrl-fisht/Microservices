namespace FileService.Contracts.Responses;

public sealed record CompleteMultipartUploadResponse
{
    public required Guid MediaAssetId { get; init; }
}