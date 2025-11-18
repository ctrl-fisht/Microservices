using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Domain.Entities;

public sealed class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120; // 5 GB
    public const string BUCKET = "videos";
    public const string RAW_PREFIX = "raw";
    public const string HLS_PREFIX = "hls";
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];
    
    [JsonPropertyName("hls_root_key")]
    public StorageKey HlsRootKey { get; private set; }
    
    private VideoAsset(MediaData mediaData, StorageKey rawKey, MediaOwner owner, StorageKey hlsRootKey) 
        : base(mediaData, AssetType.Video, rawKey, owner)
    {
        HlsRootKey = hlsRootKey;
    }

    public UnitResult<Error> CompleteProcessing(DateTime timestamp)
    {
        var appendSegmentResult = HlsRootKey.AppendSegment(MASTER_PLAYLIST_NAME);
        if (appendSegmentResult.IsFailure)
            return appendSegmentResult.Error;
        
        var markReadyResult = MarkReady(HlsRootKey, timestamp);
        if (markReadyResult.IsFailure)
            return markReadyResult.Error;

        return UnitResult.Success<Error>();
    }
    
    public static UnitResult<Error> ValidateForUpload(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension.ToLowerInvariant()))
            return Error.Validation(
                "invalid.extension", 
                $"Video extension must be one of: {string.Join(",", AllowedExtensions)}");
        if (mediaData.ContentType.MediaType != MediaType.Video)
            return Error.Validation("invalid.media.type", "Media type must be Video");
        
        if (mediaData.Size >= MAX_SIZE)
            return Error.Validation("invalid.size.", "Media size is too big (max: 5GB)");

        return UnitResult.Success<Error>();
    }

    public static Result<VideoAsset, Error> Create(
        MediaData mediaData,
        StorageKey rawKey,
        MediaOwner owner,
        StorageKey hlsRootKey)
    {
        var validateMediaDataResult = ValidateForUpload(mediaData);
        if  (!validateMediaDataResult.IsSuccess)
            return validateMediaDataResult.Error;

        return new VideoAsset(mediaData, rawKey, owner, hlsRootKey);
    }
    
}