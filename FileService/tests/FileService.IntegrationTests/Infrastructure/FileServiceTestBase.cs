using FileService.Domain.Entities;
using FileService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestBase : IClassFixture<IntegrationTestsWebFactory>
{
    public const string TEST_FILENAME = "test-file.mp4";
    public const string TEST_FILE_FOLDER = "Resources";
    protected FileServiceTestBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services = factory.Services;
    }
    
    protected async Task<T> ExecuteInDb<T>(Func<FileServiceDbContext, Task<T>> func)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        return await func(dbContext);
    }

    protected async Task<MediaAsset> GetMediaAssetFromDb(Guid mediaAssetId)
    {
        return await ExecuteInDb(async (context) =>
        {
            return await context.MediaAssets.FirstAsync(m => m.Id == mediaAssetId);
        });
    }

    protected IServiceProvider Services { get; init; }

    protected HttpClient HttpClient { get; init; }

    protected HttpClient AppHttpClient { get; init; }
}