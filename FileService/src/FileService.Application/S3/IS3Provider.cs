using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Application.S3;

public interface IS3Provider
{
    Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken);
    Task<Result<Stream, Error>> DownloadFileAsync(StorageKey key, CancellationToken cancellationToken);
    Task<UnitResult<Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken);
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey key);
    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key);
    Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken);
    
    Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey key, MediaData mediaData, CancellationToken cancellationToken);
    Task<Result<string, Error>> GenerateChunkUploadUrlAsync(StorageKey key, string uploadId, int partNumber);
    Task<Result<List<ChunkUploadUrl>, Error>> GenerateAllChunkUploadUrlsAsync(StorageKey key, string uploadId, int totalChunks, CancellationToken cancellationToken);
    Task<Result<S3CompleteMultipartUploadResponse, Error>> CompleteMultipartUploadAsync(StorageKey key, string uploadId, List<PartETagDto> etags, CancellationToken cancellationToken);
    Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey key, string uploadId, CancellationToken cancellationToken);
    Task<Result<S3ListMultipartUploadsResponse, Error>> ListMultipartUploadsAsync(string bucketName, CancellationToken cancellationToken);
}