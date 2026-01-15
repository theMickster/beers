using Beers.Application.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Beers.UnitTests.Application.Data;

public sealed class BeersMetadataDbContextConfigurationTests
{
    [Theory]
    [InlineData(typeof(BeerTypeEntity))]
    [InlineData(typeof(BeerStyleEntity))]
    [InlineData(typeof(BeerCategoryEntity))]
    [InlineData(typeof(BreweryTypeEntity))]
    public void ModelConfiguration_ShouldUseMetadataContainerAndHierarchicalPartitionKey(Type entityClrType)
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(entityClrType);

        using (new AssertionScope())
        {
            entityType.Should().NotBeNull();
            entityType!.GetContainer().Should().Be(CosmosContainerConstants.MetadataContainer);
            entityType.GetPartitionKeyPropertyNames().Should()
                .Equal(nameof(BaseMetaDataEntity.ApplicationName), nameof(BaseMetaDataEntity.TypeId));
        }
    }

    private static BeersMetadataDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BeersMetadataDbContext>()
            .UseCosmos(
                accountEndpoint: "https://localhost:8081",
                accountKey: "AQIDBAUGBwgJCgsMDQ4PEA==",
                databaseName: "Beers")
            .Options;

        return new BeersMetadataDbContext(options);
    }
}
