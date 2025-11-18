using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;

namespace FileService.Infrastructure.Postgres.JsonConverters;

public class FileNameJsonConverter : JsonConverter<FileName>
{
    public override FileName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var name = doc.RootElement.GetProperty("name").GetString()!;
        var extension = doc.RootElement.GetProperty("extension").GetString()!;
        return FileName.FromDb(name, extension);
    }

    public override void Write(Utf8JsonWriter writer, FileName value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);
        writer.WriteString("extension", value.Extension);
        writer.WriteEndObject();
    }
}