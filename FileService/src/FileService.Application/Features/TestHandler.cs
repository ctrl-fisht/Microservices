using CSharpFunctionalExtensions;
using FileService.Domain.Entities;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres;
using Shared.Kernel.Errors;

namespace FileService.Application.Features;

public class TestHandler
{
    private readonly FileServiceDbContext _dbContext;

    public TestHandler(FileServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<MediaAsset> Handle()
    {
        var mediaData = MediaData.Create(
            fileName: FileName.Create("videofile.mp4").Value,
            contentType: ContentType.Create("video/mp4").Value,
            100000,
            100).Value;
        var storageKey = StorageKey.Create("videos", "video", "user1").Value;
        var mediaOwner = MediaOwner.ForDepartment(Guid.NewGuid()).Value;
        var hlsRootKey = StorageKey.Create("videos", "video", "user1").Value;
        
        var videoAsset = VideoAsset.Create(mediaData, storageKey, mediaOwner, hlsRootKey).Value;
        videoAsset.CompleteProcessing(DateTime.UtcNow);
        await _dbContext.MediaAssets.AddAsync(videoAsset);
        await _dbContext.SaveChangesAsync();
        return _dbContext.MediaAssets.First();
    }
}