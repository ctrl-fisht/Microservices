using FileService.Core.Extensions;
using Shared.Framework.VerticalSlice;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging(builder.Configuration);
builder.Services.AddAppEndpoints(typeof(Program).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FileServiceAPI");
    });
}

app.MapGet("/", () => "Hello World!");


app.UseAppEndpoints();
app.Run();