using Serilog;

namespace DirectoryService.Presentation.Extensions;

public static class LoggingExtensions
{
    public static void AddLogging(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        var seqConnString = configuration.GetConnectionString("Seq");
        if  (string.IsNullOrEmpty(seqConnString))
            throw new Exception("Seq connection string not found");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()                  
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()       
            .Enrich.WithProperty("ServiceName", "DirectoryService")
            .WriteTo.Console()                        
            .WriteTo.Seq(seqConnString)
            .CreateLogger();

        host.UseSerilog();
    }
}