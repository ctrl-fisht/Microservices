namespace FileService.Contracts.Dtos;

public sealed record FileInfoDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
}