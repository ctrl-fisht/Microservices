using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.Entities;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres;
using Shared.Kernel.Errors;

namespace FileService.Application.Features;

public class TestHandler
{
    private readonly FileServiceDbContext _dbContext;
    private readonly IS3Provider _s3Provider;

    public TestHandler(FileServiceDbContext dbContext, IS3Provider s3Provider)
    {
        _dbContext = dbContext;
        _s3Provider = s3Provider;
    }
    
    public async Task<MediaAsset> Handle()
    {
        // var mediaData = MediaData.Create("", "", 5000, )
        // var newAsset = new VideoAsset();
        //
        // var mediaAsset = _dbContext.MediaAssets.First();
        //
        // await _s3Provider.UploadFileAsync()
        return _dbContext.MediaAssets.First();
    }
}