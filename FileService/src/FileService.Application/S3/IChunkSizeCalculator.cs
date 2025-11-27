using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Application.S3;

public interface IChunkSizeCalculator
{
    public (int TotalChunks, long ChunkSize) Calculate(long fileSize);
}