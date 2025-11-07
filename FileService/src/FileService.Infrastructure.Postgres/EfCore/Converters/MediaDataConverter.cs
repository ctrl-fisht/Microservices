using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres.JsonConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileService.Infrastructure.Postgres.EfCore.Converters;

public class MediaDataConverter : ValueConverter<MediaData, string>
{
    
    public MediaDataConverter() : base(
        md => JsonSerializer.Serialize(md, new JsonSerializerOptions
        {
            Converters = { new FileNameJsonConverter(), new ContentTypeJsonConverter(), new MediaDataJsonConverter() }
        }),
        json => JsonSerializer.Deserialize<MediaData>(json, new JsonSerializerOptions
        {
            Converters = { new FileNameJsonConverter(), new ContentTypeJsonConverter(), new MediaDataJsonConverter() }
        })!
    )
    { }
}