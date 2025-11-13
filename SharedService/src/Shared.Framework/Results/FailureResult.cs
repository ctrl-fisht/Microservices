using Microsoft.AspNetCore.Http;
using Shared.Kernel;
using Shared.Kernel.Errors;

namespace Shared.Framework.Results;

public class FailureResult : IResult
{
    private readonly Errors _errors;

    public FailureResult(Errors errors)
    {
        _errors = errors;
    }

    public FailureResult(Error error)
    {
        _errors = error.ToErrors();
    }


    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        if (!_errors.Any())
        {
            httpContext.Response.StatusCode = 500;
            return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
        }

        var errorTypes = _errors
            .Select(e => e.Type)
            .Distinct()
            .ToList();
        
        int statusCode = errorTypes.Count > 1
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeForErrorType(errorTypes.First());

        var envelope = Envelope.Error(_errors);
        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(envelope);
    }

    private static int GetStatusCodeForErrorType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

}