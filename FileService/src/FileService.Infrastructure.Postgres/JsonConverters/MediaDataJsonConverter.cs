using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;

namespace FileService.Infrastructure.Postgres.JsonConverters;

public class MediaDataJsonConverter : JsonConverter<MediaData>
{
    public override MediaData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        var fileName = JsonSerializer.Deserialize<FileName>(
            doc.RootElement.GetProperty("file_name").GetRawText(), options)!;
        var contentType = JsonSerializer.Deserialize<ContentType>(
            doc.RootElement.GetProperty("content_type").GetRawText(), options)!;
        var size = doc.RootElement.GetProperty("size").GetInt64();
        var expectedChunks = doc.RootElement.GetProperty("expected_chunks_count").GetInt32();

        return MediaData.FromDb(fileName, contentType, size, expectedChunks);
    }

    public override void Write(Utf8JsonWriter writer, MediaData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("file_name");
        JsonSerializer.Serialize(writer, value.FileName, options);
        writer.WritePropertyName("content_type");
        JsonSerializer.Serialize(writer, value.ContentType, options);
        writer.WriteNumber("size", value.Size);
        writer.WriteNumber("expected_chunks_count", value.ExpectedChunksCount);
        writer.WriteEndObject();
    }
}