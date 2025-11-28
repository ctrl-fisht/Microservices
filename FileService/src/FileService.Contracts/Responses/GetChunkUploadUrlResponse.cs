namespace FileService.Contracts.Responses;

public sealed record GetChunkUploadUrlResponse
{
    public required string UploadUrl { get; init; }
    public required int PartNumber { get; init; }
}