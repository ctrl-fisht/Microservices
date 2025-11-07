using System.Text.Json;
using System.Text.Json.Serialization;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres.JsonConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileService.Infrastructure.Postgres.EfCore.Converters;

public class MediaOwnerConverter : ValueConverter<MediaOwner, string>
{
    public MediaOwnerConverter() : base(
        mo => JsonSerializer.Serialize(mo, new JsonSerializerOptions
        {
            Converters = { new MediaOwnerJsonConverter() }
        }),
        json => JsonSerializer.Deserialize<MediaOwner>(json, new JsonSerializerOptions
        {
            Converters = { new MediaOwnerJsonConverter() }
        })!
    )
    { }
}