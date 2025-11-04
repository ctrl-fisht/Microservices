using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Domain;

public sealed record MediaData
{
    public FileName FileName { get; }
    public ContentType ContentType { get; }
    public long Size { get; }
    public int ExpectedChunksCount { get; }

    private MediaData(FileName fileName, ContentType contentType, long size, int expectedChunksCount)
    {
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        ExpectedChunksCount = expectedChunksCount;
    }

    public Result<MediaData, Error> Create(
        FileName fileName,
        ContentType contentType,
        long size,
        int expectedChunksCount)
    {
        if (size < 0)
            return Error.Failure("invalid.param", "File size cannot be negative");
        
        if (expectedChunksCount < 0)
            return Error.Failure("invalid.param", "File size cannot be negative");
        
        return new MediaData(fileName, contentType, size, expectedChunksCount);
    }
}