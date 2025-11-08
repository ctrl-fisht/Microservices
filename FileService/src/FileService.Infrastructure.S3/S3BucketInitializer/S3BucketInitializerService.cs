using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3.S3BucketInitializer;

public class S3BucketInitializerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    public S3BucketInitializerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var s3Client = _serviceProvider.GetRequiredService<IAmazonS3>();
        var s3Config = _serviceProvider.GetRequiredService<IOptions<S3Options>>().Value;
        var logger =  _serviceProvider.GetRequiredService<ILogger<S3BucketInitializer>>();
        
        var initializer = new S3BucketInitializer(s3Client, s3Config, logger);
        await initializer.Initialize();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}