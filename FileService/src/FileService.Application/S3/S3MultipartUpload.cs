namespace FileService.Application.S3;

public class S3MultipartUpload
{
    public required string Key { get; set; }
    public required string UploadId { get; set; }
}