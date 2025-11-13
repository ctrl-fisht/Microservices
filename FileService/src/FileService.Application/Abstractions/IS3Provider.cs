using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Application.Abstractions;

public interface IS3Provider
{
    Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken);
    Task<Result<Stream, Error>> DownloadFileAsync(StorageKey key, CancellationToken cancellationToken);
    Task<UnitResult<Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken);
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey key);
    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key);
    Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken);
}