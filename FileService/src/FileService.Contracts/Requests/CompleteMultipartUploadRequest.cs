using FileService.Contracts.Dtos;

namespace FileService.Contracts.Requests;

public sealed record CompleteMultipartUploadRequest
{
    public required Guid MediaAssetId { get; init; }
    public required string UploadId { get; init; }
    public required List<PartETagDto> PartETags { get; init; }
}