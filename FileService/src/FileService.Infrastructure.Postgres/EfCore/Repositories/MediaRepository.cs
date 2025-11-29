using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Domain;
using FileService.Domain.Entities;
using FileService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Errors;

namespace FileService.Infrastructure.Postgres.EfCore.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly FileServiceDbContext _context;
    private readonly ILogger<MediaRepository> _logger;
    public MediaRepository(FileServiceDbContext context, ILogger<MediaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<UnitResult<Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        await _context.MediaAssets.AddAsync(mediaAsset, cancellationToken);
        return UnitResult.Success<Error>();
    }

    public async Task<Result<MediaAsset, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var asset = await _context.MediaAssets
            .Where(x => x.Status != Status.Deleted)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if  (asset == null)
            return Error.NotFound("entity.not.found", $"Asset with given id={id} doesn't exist");
        return asset;
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError("Error occured while saving changes Exception={Exception}" ,e.Message);
            return UnitResult.Failure(Error.Failure("save.changes.error", "Error occured while saving changes"));
        }
    }

    public async Task<List<MediaAsset>> GetBatchAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        return await _context.MediaAssets
            .Where(x => x.Status != Status.Deleted)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}