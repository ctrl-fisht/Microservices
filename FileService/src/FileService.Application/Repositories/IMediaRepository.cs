using CSharpFunctionalExtensions;
using FileService.Domain.Entities;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Application.Repositories;

public interface IMediaRepository
{
    public Task AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken);
    public Task<Result<MediaAsset, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
    public Task<List<MediaAsset>> GetBatchAsync(List<Guid> ids,  CancellationToken cancellationToken);
}