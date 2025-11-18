using System.Text.Json.Serialization;

namespace Shared.Kernel.Errors;

public record Error
{
    private const string Separator = "@#$"; 
    public string Code { get; }
    public string Message { get; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorType Type { get; }
    public string? InvalidField { get; }
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public string Serialize()
    {
        return $"{Code}{Separator}{Message}{Separator}{Type}{Separator}{InvalidField}";
    }

    public static Error Deserialize(string json)
    {
        var args = json.Split(Separator);
        
        if (args.Length != 4)
            throw new InvalidOperationException("Неверный формат сериализованных данных.");
        
        return new Error(
            args[0],
            args[1],
            (ErrorType)Enum.Parse(typeof(ErrorType), args[2]),
            string.IsNullOrWhiteSpace(args[3]) ?  null : args[3]);
    }
    
    public static Error Validation(string code, string message, string?  invalidField = null)
    {
        return new Error(code, message, ErrorType.Validation, invalidField);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    public static Error Forbidden(string code, string message)
    {
        return new Error(code, message, ErrorType.Forbidden);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.Failure);
    }
    
    public Errors ToErrors()
    {
        return new Errors(this);
    }
}