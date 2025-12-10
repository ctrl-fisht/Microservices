using System.Net.Http.Json;
using CSharpFunctionalExtensions;

namespace Shared.Kernel;

public static class HttpResponseExtensions
{
    public static async Task<Result<TResponse, Errors.Errors>> ToResult<TResponse>(
        this HttpResponseMessage response, 
        CancellationToken cancellationToken = default)
    {
        Envelope<TResponse>? envelope = await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);
        if (envelope is null) throw new Exception("Response is not in envelope format");

        Result<TResponse, Errors.Errors> result;
        if (envelope.Errors is null && envelope.Result is not null)
            return envelope.Result;
        if (envelope.Errors is not null)
            return new Errors.Errors(envelope.Errors);
        throw new Exception("Response is not in envelope format");
    }
}