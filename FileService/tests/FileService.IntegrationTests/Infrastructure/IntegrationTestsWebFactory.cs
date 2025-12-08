using Amazon.S3;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.Infrastructure.S3.S3BucketInitializer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("file_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder()
        .WithImage("minio/minio")
        .WithUsername("minioadmin")
        .WithPassword("minioadmin")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json"), optional: true);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _dbContainer.GetConnectionString(),
                ["S3Options:Endpoint"] = _minioContainer.GetConnectionString()
                // ["S3Options:Endpoint"] = $"http://{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(9000)}",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<FileServiceDbContext>();
            services.AddDbContextPool<FileServiceDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(FileServiceDbContext).Assembly.FullName);
                    });
                options.EnableDetailedErrors();
            });
            services.RemoveAll<IAmazonS3>();
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
        });
    }
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _minioContainer.StartAsync();
        
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        await db.Database.MigrateAsync();

        var hostedServices = Services.GetRequiredService<IEnumerable<IHostedService>>();

        var myService = hostedServices
            .OfType<S3BucketInitializerService>()
            .First();
        // todo: инициализировал бакеты, но тоже не помогло.
        await myService.StartAsync(CancellationToken.None);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        
        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();
    }
}