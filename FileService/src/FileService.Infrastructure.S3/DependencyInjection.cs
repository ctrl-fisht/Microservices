using Amazon;
using Amazon.S3;
using FileService.Application.S3;
using FileService.Infrastructure.S3.S3BucketInitializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public static class DependencyInjection
{

    public static IServiceCollection AddS3(
        this IServiceCollection services,
        IConfiguration configuration,
        bool initializeBuckets = false)
    {
        services.Configure<S3Options>(configuration.GetRequiredSection("S3Options"));
        services.AddSingleton<IAmazonS3>(sp =>
            {
                var s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;
                var s3Config = new AmazonS3Config()
                {
                    ServiceURL = s3Options.Endpoint,
                    ForcePathStyle = true,
                    UseHttp = !s3Options.WithSsl
                };
                return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, s3Config);
            });
        
        if (initializeBuckets)
            services.AddHostedService<S3BucketInitializerService>();

        services.AddSingleton<IS3Provider, S3Provider>();
        services.AddTransient<IChunkSizeCalculator, ChunkSizeCalculator>();
        
        return services;
    }
}