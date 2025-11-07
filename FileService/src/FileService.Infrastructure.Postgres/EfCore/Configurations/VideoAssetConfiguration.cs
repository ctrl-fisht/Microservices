using FileService.Domain.Entities;
using FileService.Infrastructure.Postgres.EfCore.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.EfCore.Configurations;

public class VideoAssetConfiguration : IEntityTypeConfiguration<VideoAsset>
{
    public void Configure(EntityTypeBuilder<VideoAsset> builder)
    {
        builder.Property(v => v.HlsRootKey)
            .HasColumnName("hls_root_key")
            .HasConversion(new StorageKeyConverter()!);
    }
}