namespace FileService.Infrastructure.S3;

public class S3Options
{
    public string Endpoint { get; init; } = null!;
    public string AccessKey { get; init; } = null!;
    public string SecretKey { get; init; } = null!;
    public bool WithSsl { get; init; }
    public List<string> RequiredBuckets { get; init; } = null!;
    public int UploadUrlExpirationMinutes { get; init; }
    public int DownloadUrlExpirationHours { get; init; }
    public int MaxConcurrentRequests { get; init; }
    public bool OverwriteExistingPolicies { get; init; }

    public long RecommendedChunkSize { get; init; } = 500 * 1024 * 1024;
    public int MaxChunks { get; init; } = 100;
    
    public static string PublicPolicy(string bucket) => $@"{{
                              ""Version"": ""2012-10-17"",
                              ""Statement"": [
                                {{
                                  ""Effect"": ""Allow"",
                                  ""Principal"": {{ ""AWS"": [""*""] }},
                                  ""Action"": [""s3:GetObject""],
                                  ""Resource"": [""arn:aws:s3:::{bucket}/*""]
                                }}
                              ]
                            }}";
}