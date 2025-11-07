using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;

namespace FileService.Infrastructure.Postgres.JsonConverters;

public class StorageKeyJsonConverter : JsonConverter<StorageKey>
{
    public override StorageKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        try
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            string bucket = doc.RootElement.GetProperty("bucket").GetString()!;
            string? prefix = doc.RootElement.GetProperty("prefix").GetString();
            string key = doc.RootElement.GetProperty("key").GetString()!;
            return StorageKey.FromDb(bucket, prefix, key);
        }
        catch
        {
            return null; 
        }
    }

    public override void Write(Utf8JsonWriter writer, StorageKey value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("bucket", value.Bucket);
        writer.WriteString("prefix", value.Prefix);
        writer.WriteString("key", value.Key);
        writer.WriteEndObject();
    }
}