namespace FileService.Application.S3;

public record S3ListMultipartUploadsResponse
{
    public required string Bucket { get; init; }
    public required List<S3MultipartUpload> MultipartUploads { get; init; }
}