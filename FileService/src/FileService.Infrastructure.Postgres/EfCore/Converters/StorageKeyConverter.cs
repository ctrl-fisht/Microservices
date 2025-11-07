using System.Text.Json;
using FileService.Domain.ValueObjects;
using FileService.Infrastructure.Postgres.JsonConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileService.Infrastructure.Postgres.EfCore.Converters;

public class StorageKeyConverter : ValueConverter<StorageKey?, string>
{
    public StorageKeyConverter() : base(
        sk => JsonSerializer.Serialize(sk, new JsonSerializerOptions 
            { Converters = { new StorageKeyJsonConverter() }}),
        json => JsonSerializer.Deserialize<StorageKey>(json, new JsonSerializerOptions
        {
            Converters = { new StorageKeyJsonConverter() }
        })!
    )
    { }
}