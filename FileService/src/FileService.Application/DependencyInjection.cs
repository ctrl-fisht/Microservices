using FileService.Application.S3;
using FileService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
namespace FileService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddSingleton<MediaOwnerFactory>();
        services.AddSingleton<MediaAssetFactory>();
        
        services.AddScoped<Features.StartMultipartUpload.Handler>();
        services.AddScoped<Features.GetChunkUploadUrl.Handler>();
        services.AddScoped<Features.CompleteMultipartUpload.Handler>();
        services.AddScoped<Features.AbortMultipartUpload.Handler>();
        return services;
    }
}