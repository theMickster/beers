using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data.Configurations;

internal class BrewerEntityConfiguration : BaseEntityConfiguration, IEntityTypeConfiguration<BrewerEntity>
{
    public void Configure(EntityTypeBuilder<BrewerEntity> builder)
    {
        BeerConfiguring(builder);

        builder.HasDiscriminator(x => x.EntityType).HasValue(PartitionKeyConstants.Brewer);
        builder.OwnsOne(x => x.BreweryType);

        //builder.Property(x => x.Name).ToJsonProperty("name");
        //builder.Property(x => x.FoundedIn).ToJsonProperty("foundedIn");
        //builder.Property(x => x.Headquarters).ToJsonProperty("headquarters");
        //builder.Property(x => x.Website).ToJsonProperty("website");
        //builder.Property(x => x.CreatedDate).ToJsonProperty("createdDate");
        //builder.Property(x => x.ModifiedDate).ToJsonProperty("modifiedDate");
        
    }
}
