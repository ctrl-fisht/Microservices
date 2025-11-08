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
    public bool OverwriteExistingPolicies { get; set; }
    
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