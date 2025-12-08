namespace FileService.Contracts.Responses;

public sealed record StartMultipartUploadResponse
{
    public Guid MediaAssetId { get; init; } 
    public required string UploadId { get; init; }
    public required List<ChunkUploadUrlDto> ChunkUrls { get; init; }
    public int ChunkSize { get; init; }
}

public sealed record ChunkUploadUrlDto
{
    public int PartNumber { get; init; }
    public required string UploadId { get; init; }
    public required string Value { get; init; }
};
