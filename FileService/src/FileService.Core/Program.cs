using FileService.Core.Extensions;
using Shared.Framework.VerticalSlice;
using Prometheus;
using Serilog;
using Shared.Framework.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging(builder.Configuration);
builder.Services.AddAppEndpoints(typeof(Program).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseRequestCorrelationId();
app.UseSerilogRequestLogging();
app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FileServiceAPI");
    });
}


app.MapGet("/", () => "Hello World!");
app.MapMetrics();

app.UseAppEndpoints();
app.Run();