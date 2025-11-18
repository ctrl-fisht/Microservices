using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;

namespace FileService.Infrastructure.Postgres.JsonConverters;

public class MediaOwnerJsonConverter : JsonConverter<MediaOwner>
{
    public override MediaOwner Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        var context = doc.RootElement.GetProperty("context").GetString()!;
        var entityId = doc.RootElement.GetProperty("entity_id").GetGuid();

        return MediaOwner.FromDb(context, entityId);
    }

    public override void Write(Utf8JsonWriter writer, MediaOwner value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("context", value.Context);
        writer.WriteString("entity_id", value.EntityId.ToString());
        writer.WriteEndObject();
    }
}