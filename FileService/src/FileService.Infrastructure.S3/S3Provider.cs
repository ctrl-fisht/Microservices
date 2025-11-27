using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Application.S3;
using FileService.Contracts.Dtos;
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
    private readonly SemaphoreSlim _requestsSemaphore;
    
    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
        _requestsSemaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
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
        catch (Exception e)
        {
            _logger.LogError(
                "Error while uploading file {FileName}, Exception={Exception}",
                mediaData.FileName.Full, e.Message);
            return e.MapToError();
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
        catch(Exception e)
        {
            _logger.LogError(
                "Error while downloading asset {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return e.MapToError();
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
        catch (Exception e)
        {
            _logger.LogError("Error while deleting asset {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return e.MapToError();
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
        catch (Exception e)
        {
            _logger.LogError("Error while creating upload url {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return e.MapToError();
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
        catch (Exception e)
        {
            _logger.LogError("Error while creating download url {AssetPath}, Exception={Exception}",
                key.FullPath, e.Message);
            return e.MapToError();
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating download urls, Exception={Exception}", e.Message);
            return e.MapToError();
        }
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey key, MediaData mediaData, CancellationToken cancellationToken)
    {
        var request = new InitiateMultipartUploadRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            ContentType = mediaData.ContentType.MimeType
        };
        try
        {
            var response = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);
            return response.UploadId;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while starting multipart upload, Exception={Exception}", e.Message);
            return e.MapToError();
        }
    }

    public async Task<Result<string, Error>> GenerateChunkUploadUrlAsync(StorageKey key, string uploadId, int partNumber)
    {
        var request = new GetPreSignedUrlRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            Expires = DateTime.UtcNow.AddMinutes(_options.UploadUrlExpirationMinutes),
            Verb = HttpVerb.PUT,
            Protocol = _options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
            PartNumber = partNumber,
            UploadId = uploadId
        };
        try
        {
            var response = await _s3Client.GetPreSignedURLAsync(request);
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while generating chunk url, Exception {Exception}", e.Message);
            return e.MapToError();
        }
    }

    public async Task<Result<List<ChunkUploadUrl>, Error>> GenerateAllChunkUploadUrlsAsync(StorageKey key, string uploadId, int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            var tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var request = new GetPreSignedUrlRequest()
                        {
                            BucketName = key.Bucket,
                            Key = key.Value,
                            Expires = DateTime.UtcNow.AddMinutes(_options.UploadUrlExpirationMinutes),
                            Verb = HttpVerb.PUT,
                            Protocol = _options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                            PartNumber = partNumber,
                            UploadId = uploadId
                        };
                        string url = await _s3Client.GetPreSignedURLAsync(request);
                        return new ChunkUploadUrl(partNumber, uploadId, url);
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while generating chunk upload urls, Exception {Exception}", e.Message);
            return e.MapToError();
        }
    }

    public async Task<Result<S3CompleteMultipartUploadResponse, Error>> CompleteMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        List<PartETagDto> etags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest()
            {
                BucketName = key.Bucket,
                Key = key.Value,
                UploadId = uploadId,
                PartETags = new List<Amazon.S3.Model.PartETag>(
                    etags
                        .Select(etag => new Amazon.S3.Model.PartETag(etag.PartNumber, etag.ETag))).ToList()
            };
            var response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);
            return new S3CompleteMultipartUploadResponse();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while completing multipart upload, Exception={Exception}", e.Message);
            return e.MapToError();
        }
    }

    public async Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey key, string uploadId, CancellationToken cancellationToken)
    {
        try
        {
            var request = new AbortMultipartUploadRequest()
            {
                BucketName = key.Bucket,
                Key = key.Value,
                UploadId = uploadId
            };
            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while abort multipart upload, Exception={Exception}", e.Message);
            return e.MapToError();
        }
    }

    public async Task<Result<S3ListMultipartUploadsResponse, Error>> ListMultipartUploadsAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var request = new ListMultipartUploadsRequest()
            {
                BucketName = bucketName
            };
            var result = await _s3Client.ListMultipartUploadsAsync(request, cancellationToken);
            return new S3ListMultipartUploadsResponse()
            {
                Bucket = bucketName,
                MultipartUploads = result.MultipartUploads.Select(mu =>
                    new S3MultipartUpload()
                    {
                        Key = mu.Key,
                        UploadId = mu.UploadId
                    }).ToList()
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while listing multipart upload, Exception={Exception}", e.Message);
            return e.MapToError();
        }
    }
}