using CSharpFunctionalExtensions;
using FileService.Domain.Entities;
using Shared.Kernel.Errors;

namespace FileService.Application.Repositories;

public interface IMediaRepository
{
    public UnitResult<Error> CreateAsync(MediaAsset mediaAsset, CancellationToken cancellationToken);
    public Result<MediaAsset, Error> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public UnitResult<Error> SaveChangesAsync(CancellationToken cancellationToken);
}