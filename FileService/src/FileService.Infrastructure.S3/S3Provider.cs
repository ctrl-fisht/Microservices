using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Kernel.Errors;

namespace FileService.Infrastructure.S3;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;
    private readonly S3Options _options;
    
    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }
    
    public async Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken)
    {
        var putRequest = new PutObjectRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            InputStream = stream,
            ContentType = mediaData.ContentType.MimeType,
        };

        try
        {
            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Error uploading file, Exception={Exception}", amazonS3Exception);
            return amazonS3Exception.MapToError(key);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error while uploading file {FileName}, Exception={Exception}",
                mediaData.FileName.Full, e.Message);
            return Error.Failure("error.while.upload", $"Error while uploading file {mediaData.FileName.Full}");
        }
    }

    public async Task<Result<Stream, Error>> DownloadFileAsync(StorageKey key, CancellationToken cancellationToken)
    {
        var getRequest = new GetObjectRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
        };
        try
        {
            var asset = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
            var memoryStream = new MemoryStream();
            await asset.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Error downloading file, Exception={Exception}", amazonS3Exception);
            return amazonS3Exception.MapToError(key);
        }
        catch(Exception e)
        {
            _logger.LogError(
                "Error while downloading asset {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return Error.Failure("error.while.download", $"Error while downloading file '{key.FullPath}");
        }
        
    }

    public async Task<UnitResult<Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken)
    {
        var deleteRequest = new DeleteObjectRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value
        };

        try
        {
            var result = await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Error while deleting asset, Exception={Exception}", amazonS3Exception);
            return amazonS3Exception.MapToError(key);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deleting asset {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return Error.Failure("error.while.delete", $"Error while deleting file '{key.FullPath}'");
        }
    }

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey key)
    {
        var request = new GetPreSignedUrlRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            Expires = DateTime.UtcNow.AddMinutes(_options.UploadUrlExpirationMinutes),
            Verb = HttpVerb.PUT,
            Protocol = _options.WithSsl ? Protocol.HTTPS :  Protocol.HTTP
        };
        try
        {
            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Error while creating upload url, Exception={Exception}", amazonS3Exception);
            return amazonS3Exception.MapToError(key);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while creating upload url {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return Error.Failure("error.presigned.upload", $"Error while creating upload url '{key.FullPath}'");
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key)
    {
        var request = new GetPreSignedUrlRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            Expires = DateTime.UtcNow.AddHours(_options.DownloadUrlExpirationHours),
            Verb = HttpVerb.PUT,
            Protocol = _options.WithSsl ? Protocol.HTTPS :  Protocol.HTTP
        };
        try
        {
            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Error while creating download url, Exception={Exception}", amazonS3Exception);
            return amazonS3Exception.MapToError(key);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while creating download url {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return Error.Failure("error.presigned.download", $"Error while creating download url '{key.FullPath}'");
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> keys,
        CancellationToken cancellationToken)
    {
        var urls = new List<string>();

        try
        {
            foreach (var key in keys)
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = key.Bucket,
                    Key = key.Value,
                    Expires = DateTime.UtcNow.AddHours(_options.DownloadUrlExpirationHours),
                    Verb = HttpVerb.PUT,
                    Protocol = _options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                };

                var url = await _s3Client.GetPreSignedURLAsync(request);
                urls.Add(url);
            }

            return urls;
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Error while creating download urls, Exception={Exception}", amazonS3Exception);
            return amazonS3Exception.MapToError(StorageKey.None);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating download urls, Exception={Exception}", e.Message);
            return Error.Failure("error.presigned.download.batch", "Error while creating one or more download urls");
        }
    }
}