namespace FileService.Contracts.Requests;

public sealed record GetChunkUploadUrlRequest
{
    public required Guid MediaAssetId { get; init; }
    public required string UploadId { get; init; }
    public required int PartNumber { get; init; }
}