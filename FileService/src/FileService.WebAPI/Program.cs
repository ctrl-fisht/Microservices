using FileService.Application;
using FileService.WebAPI.Extensions;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Shared.Framework.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddPostgresEfCore(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddS3(builder.Configuration, initializeBuckets: true);

var app = builder.Build();
app.UseRequestCorrelationId();
app.UseSerilogRequestLogging();


app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    using (IServiceScope scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FileServiceAPI");
    });
}


app.MapControllers();
app.MapMetrics();

// app.UseAppEndpoints();
app.Run();