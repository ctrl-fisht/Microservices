using CSharpFunctionalExtensions;
using FileService.Application.Repositories;
using FileService.Application.S3;
using FileService.Domain;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.DeleteMediaAsset;

public class Handler
{ 
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    public Handler(
        IMediaRepository mediaRepository,
        IS3Provider s3Provider)
    {
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        var mediaAssetResult = await _mediaRepository.GetByIdAsync(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();
        var asset = mediaAssetResult.Value;

        var deletedResult = asset.MarkDeleted();
        if (deletedResult.IsFailure)
            return deletedResult.Error.ToErrors();
        
        var deleteRawTask = _s3Provider.DeleteFileAsync(asset.RawKey, cancellationToken);
        var deleteFinalTask = asset.FinalKey != null 
            ? _s3Provider.DeleteFileAsync(asset.FinalKey, cancellationToken) 
            : null;
        Task[] tasks = [deleteRawTask, deleteFinalTask ?? Task.CompletedTask];
        await Task.WhenAll(tasks);

        var saveChangesResult = await _mediaRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        return mediaAssetId;
    }
}