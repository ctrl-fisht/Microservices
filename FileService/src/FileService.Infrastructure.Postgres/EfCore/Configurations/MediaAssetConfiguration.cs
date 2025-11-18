using FileService.Domain;
using FileService.Domain.Entities;
using FileService.Infrastructure.Postgres.EfCore.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.EfCore.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired();
        
        builder.HasDiscriminator(a => a.AssetType)
            .HasValue<VideoAsset>(AssetType.Video)
            .HasValue<PreviewAsset>(AssetType.Preview);

        builder.Property(a => a.AssetType)
            .HasColumnName("asset_type")
            .HasConversion(
                v => v.ToString(),
                str => Enum.Parse<AssetType>(str))
            .IsRequired();
        
        
        builder.Property(a => a.MediaData)
            .HasColumnName("media_data")
            .HasColumnType("jsonb")
            .HasConversion(new MediaDataConverter())
            .IsRequired();
        
        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                str => Enum.Parse<Status>(str))
            .IsRequired();
        
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder.Property(a => a.RawKey)
            .HasColumnName("raw_key")
            .HasConversion(new StorageKeyConverter()!)
            .IsRequired();

        builder.Property(a => a.FinalKey)
            .HasColumnName("final_key")
            .HasConversion(new StorageKeyConverter());

        builder.Property(a => a.Owner)
            .HasColumnName("owner")
            .HasConversion(new MediaOwnerConverter())
            .IsRequired();
    }
}