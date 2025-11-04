using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Domain.Entities;

public sealed class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 10_485_760; // 10 MB
    public const string BUCKET = "preview";
    public const string RAW_PREFIX = "raw";
    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];
    
    public PreviewAsset(MediaData mediaData, StorageKey rawKey, MediaOwner owner) 
        : base(mediaData, AssetType.Preview, rawKey, owner)
    {
        
    }
    
    public UnitResult<Error> CompleteUpload(DateTime timestamp)
    {
        MarkUploaded(timestamp);
        MarkReady(RawKey, timestamp);
        return UnitResult.Success<Error>();
    }
    
    public static UnitResult<Error> ValidateForUpload(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension.ToLowerInvariant()))
            return Error.Validation(
                "invalid.extension", 
                $"Video extension must be one of: {string.Join(",", AllowedExtensions)}");
        if (mediaData.ContentType.MediaType != MediaType.Image)
            return Error.Validation("invalid.media.type", "Media type must be Image");
        
        if (mediaData.Size >= MAX_SIZE)
            return Error.Validation("invalid.size.", "Media size is too big (max: 10 MB)");
        
        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> Create(MediaData mediaData, StorageKey rawKey, MediaOwner owner)
    {
        var validateMediaDataResult = ValidateForUpload(mediaData);
        if (!validateMediaDataResult.IsSuccess)
            return validateMediaDataResult.Error;

        return new PreviewAsset(mediaData, rawKey, owner);
    }

}