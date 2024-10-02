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
    }
}
