using Shared.Framework.Results;
using Shared.Framework.VerticalSlice;

namespace FileService.Core.Features.Test;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/test", async (Handler handler) =>
        {
            return await handler.Handle();
        });
    }
}

public class Handler()
{
    public async Task<EndpointResult<Guid>> Handle()
    {
        return Guid.NewGuid();
    }
}


