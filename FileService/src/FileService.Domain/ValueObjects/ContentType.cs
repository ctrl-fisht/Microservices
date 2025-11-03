using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Domain.ValueObjects;

public sealed record ContentType
{
    public MediaType Value { get;}

    private ContentType(MediaType value)
    {
        Value = value;
    }

    public Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return Error.Failure("invalid.argument", "ContentType cannot be empty");

        var type = contentType switch
        {
            _ when contentType.Contains("video") => MediaType.Video,
            _ when contentType.Contains("audio") => MediaType.Audio,
            _ when contentType.Contains("image") => MediaType.Image,
            _ => MediaType.Unknown
        };

        return new ContentType(type);
    }
}