using CSharpFunctionalExtensions;
using FileService.Application.S3;
using Microsoft.Extensions.Options;
using Shared.Kernel.Errors;

namespace FileService.Infrastructure.S3;

public class ChunkSizeCalculator : IChunkSizeCalculator
{
    private readonly S3Options _options;

    public ChunkSizeCalculator(IOptions<S3Options> options)
    {
        _options = options.Value;
    }

    public (int TotalChunks, int ChunkSize) Calculate(long fileSize)
    {
        if (fileSize <= _options.RecommendedChunkSize)
            return (1, (int)fileSize);

        int calculated = (int)Math.Ceiling((double)fileSize / _options.RecommendedChunkSize);
        
        var totalChunks = Math.Min(calculated, _options.MaxChunks);
        long chunkSize = fileSize / totalChunks;
        return (totalChunks, (int)chunkSize);
    }
}