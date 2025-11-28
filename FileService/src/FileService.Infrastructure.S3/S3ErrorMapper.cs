using Amazon.S3;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Infrastructure.S3;

public static class S3ErrorMapper
{
    public static Error MapToError(this Exception ex)
    {
        if (ex is AmazonS3Exception s3)
        {
            return s3.ErrorCode switch
            {
                "NoSuchBucket"       => Error.NotFound("s3.no.bucket", $"Bucket not found"),
                "NoSuchKey"          => Error.NotFound("s3.no.key", $"Key not found"),
                "AccessDenied"       => Error.Forbidden("s3.access.denied", $"Access to was denied"),
                "InvalidObjectState" => Error.Conflict("s3.invalid.state", $"Invalid object state'"),

                _ => Error.Failure("s3.unknown.error", 
                    $"Unknown S3 error '{s3.ErrorCode}'")
            };
        }

        if (ex is HttpRequestException)
            return Error.Failure("network.http", "Network error while connecting to S3");

        if (ex is TimeoutException)
            return Error.Failure("network.timeout", "Network timeout while communicating with S3");

        return Error.Failure("unexpected.error", ex.Message);
    }
}