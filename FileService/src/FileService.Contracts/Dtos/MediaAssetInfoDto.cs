namespace FileService.Contracts.Dtos;

public sealed record MediaAssetInfoDto
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public required string AssetType { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required FileInfoDto FileInfo { get; init; }
    public required string? DownloadUrl { get; init; }
}