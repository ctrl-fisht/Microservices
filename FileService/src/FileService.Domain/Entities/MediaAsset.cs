using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Domain.Entities;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }
    public MediaData MediaData { get; protected set; }
    public AssetType AssetType { get; protected set; }
    public Status Status { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public StorageKey RawKey { get; protected set; }
    public StorageKey? FinalKey { get; protected set; }
    public MediaOwner Owner { get; protected set; }

    protected MediaAsset(MediaData mediaData, AssetType assetType, StorageKey rawKey, MediaOwner owner)
    {
        var dateTime = DateTime.UtcNow;
        Id = Guid.NewGuid();
        MediaData = mediaData;
        AssetType = assetType;
        Status = Status.Loading;
        CreatedAt = dateTime;
        UpdatedAt = dateTime;
        RawKey = rawKey;
        FinalKey = null;
        Owner = owner;
    }
    
    public UnitResult<Error> MarkUploaded(DateTime dateTime)
    {
        if (Status is not Status.Loading)
            return UnitResult.Failure(
                Error.Validation(
                    "change.status.error",
                    "Asset must be 'loading' before mark it uploaded"));

        if (Status is Status.Uploaded)
            return UnitResult.Success<Error>();
        
        Status = Status.Uploaded;
        UpdatedAt = dateTime;
        return UnitResult.Success<Error>();

    }

    public UnitResult<Error> MarkReady(StorageKey finalKey, DateTime dateTime)
    {
        if (Status is not Status.Uploaded)
            return UnitResult.Failure<Error>(
                Error.Validation(
                    "change.status.error",
                    "Asset must be 'uploaded' before mark it ready"));
        
        if (Status is Status.Ready)
            return UnitResult.Success<Error>();
        
        Status = Status.Ready;
        FinalKey = finalKey;
        UpdatedAt = dateTime;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MarkFailed(DateTime dateTime)
    {
        if (Status is not Status.Uploaded)
            return UnitResult.Failure<Error>(
                Error.Validation(
                    "change.status.error",
                    "Asset must be 'uploaded' before mark it failed"));
        
        if (Status is Status.Failed)
            return UnitResult.Success<Error>();
        
        Status = Status.Failed;
        UpdatedAt = dateTime;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MarkDeleted(DateTime dateTime)
    {
        Status = Status.Deleted;
        UpdatedAt = dateTime;
        return UnitResult.Success<Error>();
    }
}