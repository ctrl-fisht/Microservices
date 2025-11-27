namespace FileService.Contracts.Requests;

public sealed record AbortMultipartUploadRequest
{
    public required Guid MediaAssetId { get; init; }
    public required string UploadId { get; init; }
}