namespace FileService.Contracts.Requests;

public sealed record StartMultipartUploadRequest
{
    public required string FileName { get; init; }
    public required string AssetType { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
    public required string Context { get; init; }
    public required Guid ContextId { get; init; }
}