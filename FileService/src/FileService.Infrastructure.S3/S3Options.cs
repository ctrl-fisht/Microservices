namespace FileService.Infrastructure.S3;

public class S3Options
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public bool WithSsl { get; set; }
    public List<string> RequiredBuckets { get; set; } = null!;
    public int UploadUrlExpirationMinutes { get; set; }
    public int DownloadUrlExpirationHours { get; set; }
    public int MaxConcurrentRequests { get; set; }
}