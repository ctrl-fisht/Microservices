using Serilog;

namespace FileService.Core.Extensions;

public static class LoggingExtensions
{
    public static void AddLogging(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        var seqConnString = configuration.GetConnectionString("Seq");
        if (string.IsNullOrWhiteSpace(seqConnString))
            throw new ArgumentNullException(nameof(configuration), "Cannot find seq Connection String");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()                       
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", "FileService")
            .WriteTo.Console()                                
            .WriteTo.Seq(seqConnString)                 
            .CreateLogger();

        host.UseSerilog();
    }
}