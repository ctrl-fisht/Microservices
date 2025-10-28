using Serilog;

namespace DirectoryService.Presentation.Extensions;

public static class LoggingExtensions
{
    public static void AddLogging(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()                  
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()       
            .Enrich.WithProperty("ServiceName", "DirectoryService")
            .WriteTo.Console()                                
            .CreateLogger();

        host.UseSerilog();
    }
}