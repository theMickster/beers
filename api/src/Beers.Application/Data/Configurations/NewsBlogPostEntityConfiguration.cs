using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data.Configurations;

internal sealed class NewsBlogPostEntityConfiguration : BaseEntityConfiguration, IEntityTypeConfiguration<NewsBlogPostEntity>
{
    public void Configure(EntityTypeBuilder<NewsBlogPostEntity> builder)
    {
        BeerConfiguring(builder);
        builder.HasDiscriminator(x => x.EntityType).HasValue(PartitionKeyConstants.NewsBlogPost);
        builder.Property(x => x.PostType).HasConversion<string>();

        builder.OwnsOne(x => x.Author, buildAction =>
        {
            buildAction.ToJsonProperty("Author");
            buildAction.Property(x => x.Id).ToJsonProperty("id");
            buildAction.Property(x => x.Name).ToJsonProperty("name");
            buildAction.Property(x => x.Website).ToJsonProperty("website");
        });
    }
}
