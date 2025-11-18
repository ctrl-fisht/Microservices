using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;

namespace FileService.Infrastructure.Postgres.JsonConverters;

public class ContentTypeJsonConverter : JsonConverter<ContentType>
{
    public override ContentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var mimeType = doc.RootElement.GetProperty("mime_type").GetString()!;
        var mediaType = Enum.Parse<MediaType>(doc.RootElement.GetProperty("media_type").GetString()!);
        return ContentType.FromDb(mimeType, mediaType);
    }

    public override void Write(Utf8JsonWriter writer, ContentType value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("mime_type", value.MimeType);
        writer.WriteString("media_type", value.MediaType.ToString());
        writer.WriteEndObject();
    }
}