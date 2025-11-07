using FileService.Application.Features;
using Shared.Framework.Results;
using Shared.Framework.VerticalSlice;

namespace FileService.WebAPI.Features.Test;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/test", (TestHandler handler) => handler.Handle() );
    }
}

