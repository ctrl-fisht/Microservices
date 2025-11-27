namespace FileService.Contracts.Responses;

public sealed record AbortMultipartUploadResponse
{
    public required bool Success { get; init; }   
}