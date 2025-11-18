using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Domain.ValueObjects;

public sealed record ContentType
{
    
    public string MimeType { get;}
    
    public MediaType MediaType { get;}
    private ContentType(string mimeType, MediaType mediaType)
    {
        MimeType = mimeType;
        MediaType = mediaType;
    }

    public static Result<ContentType, Error> Create(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return Error.Failure("invalid.argument", "ContentType cannot be empty");

        var mediaType = mimeType switch
        {
            _ when mimeType.Contains("video") => MediaType.Video,
            _ when mimeType.Contains("audio") => MediaType.Audio,
            _ when mimeType.Contains("image") => MediaType.Image,
            _ => MediaType.Unknown
        };

        return new ContentType(mimeType, mediaType);
    }

    public static ContentType FromDb(string mimeType, MediaType mediaType) => new(mimeType, mediaType);
}