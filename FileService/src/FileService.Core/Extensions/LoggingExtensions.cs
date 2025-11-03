using Serilog;

namespace FileService.Core.Extensions;

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
            .Enrich.WithProperty("ServiceName", "FileService")
            .WriteTo.Seq(seqConnString)
            .WriteTo.Console()                                
            .CreateLogger();

        host.UseSerilog();
    }
}