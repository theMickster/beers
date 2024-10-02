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
        entityTypeBuilder.HasPartitionKey(x => new { x.BrewerId, x.EntityType });
        entityTypeBuilder.ToContainer(CosmosContainerConstants.MainContainer);
        entityTypeBuilder.HasKey(x => x.Id);
    }
}