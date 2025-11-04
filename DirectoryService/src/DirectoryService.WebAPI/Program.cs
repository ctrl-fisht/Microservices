using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Presentation.Extensions;
using Serilog;
using Shared.Framework;
using Prometheus;
using Shared.Framework.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

var app = builder.Build();
app.UseRequestCorrelationId();
app.UseHttpMetrics();
app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionsMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DirectoryServiceAPI");
    });
}

app.MapControllers();
app.MapMetrics();
app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program { }
}