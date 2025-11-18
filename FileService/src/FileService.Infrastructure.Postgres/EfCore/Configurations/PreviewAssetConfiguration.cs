using FileService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.EfCore.Configurations;

public class PreviewAssetConfiguration : IEntityTypeConfiguration<PreviewAsset>
{
    public void Configure(EntityTypeBuilder<PreviewAsset> builder)
    {
        
    }
}