using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data.Configurations;

internal class BrewerReviewEntityConfiguration : BaseEntityConfiguration, IEntityTypeConfiguration<BrewerReviewEntity>
{
    public void Configure(EntityTypeBuilder<BrewerReviewEntity> builder)
    {
        BeerConfiguring(builder);
        builder.HasDiscriminator(x => x.EntityType).HasValue(PartitionKeyConstants.BrewerReview);
    }
}
