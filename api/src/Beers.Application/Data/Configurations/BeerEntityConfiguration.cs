using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data.Configurations;

internal class BeerEntityConfiguration : BaseEntityConfiguration, IEntityTypeConfiguration<BeerEntity>
{
    public void Configure(EntityTypeBuilder<BeerEntity> builder)
    {
        BeerConfiguring(builder);
        builder.HasDiscriminator(x => x.EntityType).HasValue(PartitionKeyConstants.Beer);
        builder.OwnsMany(x => x.Pricing, buildAction =>
        {
            buildAction.ToJsonProperty("Pricing");
            buildAction.Property(x => x.Price).ToJsonProperty("price");
            buildAction.Property(x => x.Quantity).ToJsonProperty("quantity");
            buildAction.Property(x => x.UnitVolume).ToJsonProperty("unitVolume");
            buildAction.Property(x => x.Packaging).ToJsonProperty("packaging");
        });
        
        builder.OwnsOne(x => x.Brewer, buildAction =>
        {
            buildAction.ToJsonProperty("Brewer");
            buildAction.Property(x => x.Id).ToJsonProperty("id");
            buildAction.Property(x => x.Name).ToJsonProperty("name");
            buildAction.Property(x => x.Website).ToJsonProperty("website");
        });

        builder.OwnsOne(x => x.Rating, buildAction =>
        {
            buildAction.ToJsonProperty("Rating");
            buildAction.Property(x => x.Average).ToJsonProperty("average");
            buildAction.Property(x => x.ReviewCount).ToJsonProperty("reviews");
        });

        builder.OwnsOne(x => x.BeerType, buildAction =>
        {
            buildAction.ToJsonProperty("BeerType");
            buildAction.Property(x => x.MetadataId).ToJsonProperty("metadataId");
            buildAction.Property(x => x.Name).ToJsonProperty("name");
        });

        builder.OwnsMany(x => x.BeerStyles, buildAction =>
        {
            buildAction.ToJsonProperty("BeerStyles");
            buildAction.Property(x => x.MetadataId).ToJsonProperty("metadataId");
            buildAction.Property(x => x.Name).ToJsonProperty("name");
        });

        builder.OwnsMany(x => x.BeerCategories, buildAction =>
        {
            buildAction.ToJsonProperty("BeerCategories");
            buildAction.Property(x => x.MetadataId).ToJsonProperty("metadataId");
            buildAction.Property(x => x.Name).ToJsonProperty("name");
        });
    }
}
