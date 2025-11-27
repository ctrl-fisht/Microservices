namespace FileService.Application.S3;

public record ChunkUploadUrl(int PartNumber, string UploadId, string Value);