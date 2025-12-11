using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Communication;

public static class FileServiceExtensions
{
    public const string FS_RETRY_POLICY_NAME = "FileServiceRetryPolicy";
    public static IServiceCollection AddFileServiceHttpCommunication(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<FileServiceOptions>(config.GetRequiredSection("FileServiceOptions"));
        var options = config.GetRequiredSection("FileServiceOptions").Get<FileServiceOptions>();
        
        services.AddHttpClient<IFileService, FileServiceHttpClient>(x =>
        {
            x.BaseAddress = new Uri(options!.Url);
        })
        .AddPolicyHandlerFromRegistry(FS_RETRY_POLICY_NAME);

        return services;
    }
}