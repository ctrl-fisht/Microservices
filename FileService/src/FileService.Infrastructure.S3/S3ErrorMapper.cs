using Amazon.S3;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Infrastructure.S3;

public static class S3ErrorMapper
{
    public static Error MapToError(this AmazonS3Exception ex, StorageKey key)
    {
        return ex.ErrorCode switch
        {
            "NoSuchBucket" => Error.NotFound("s3.no.bucket", $"Bucket '{key.Bucket}' not found"),
            "NoSuchKey" => Error.NotFound("s3.no.key", $"Key '{key.Value}' not found"),
            "AccessDenied " => Error.Forbidden("s3.access.denied", $"Access to '{key.FullPath}' was denied"),
            "InvalidObjectState" => Error.Conflict("s3.invalid.state", $"Invalid object state '{key.FullPath}'"),
            _ => Error.Failure("s3.unknown.error", $"Something while operation with '{key.FullPath}'")
        };
    }
}