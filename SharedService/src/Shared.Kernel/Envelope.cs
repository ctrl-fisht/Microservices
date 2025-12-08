using System.Text.Json.Serialization;
using Shared.Kernel.Errors;

namespace Shared.Kernel;

public record Envelope
{
    public object? Result { get;}
    public List<Error>? Errors { get;}

    public bool IsSuccess() => Errors == null;
    
    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, List<Error>? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.Now;
    }

    public static Envelope Ok(object? result = null) => new Envelope(result, null);
    public static Envelope Error(List<Error> errors) => new Envelope(null, errors);
}

public record Envelope<T>
{
    public T? Result { get; }
    public List<Error>? Errors { get; }
    public bool IsSuccess() => Errors == null;
    public DateTime TimeGenerated { get; }
    
    private Envelope(T? result, List<Error>? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.Now;
    }

    [JsonConstructor]
    private Envelope(T? result, List<Error>? errors, DateTime timeGenerated)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = timeGenerated;
    }

    public static Envelope<T> Ok(T? result = default) => new Envelope<T>(result, null);
    public static Envelope<T> Error(List<Error> errors) => new Envelope<T>(default, errors);
}