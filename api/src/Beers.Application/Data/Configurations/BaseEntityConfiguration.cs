using Beers.Common.Constants;
using Beers.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data.Configurations;

internal class BaseEntityConfiguration
{
    protected static void BeerConfiguring<T>(EntityTypeBuilder<T> entityTypeBuilder) where T : BaseBeerEntity
    {
        entityTypeBuilder.Property(x => x.Id).ToJsonProperty("id");
        entityTypeBuilder.ToContainer(CosmosContainerConstants.MainContainer);
        entityTypeBuilder.HasPartitionKey(x => x.BrewerId);
        entityTypeBuilder.HasKey(x => x.Id);
    }
}
