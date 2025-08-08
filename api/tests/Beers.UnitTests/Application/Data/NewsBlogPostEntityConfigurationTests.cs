using Beers.Application.Data;
using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Beers.UnitTests.Application.Data;

public sealed class NewsBlogPostEntityConfigurationTests
{
    [Fact]
    public void DbContext_ShouldExposeNewsBlogPostEntitySet()
    {
        typeof(IBeersDbContext).GetProperty(nameof(IBeersDbContext.NewsBlogPostEntities)).Should().NotBeNull();
        typeof(BeersDbContext).GetProperty(nameof(BeersDbContext.NewsBlogPostEntities)).Should().NotBeNull();
    }

    [Fact]
    public void ModelConfiguration_ShouldUseSharedContainerDiscriminatorAndPartitionKey()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(NewsBlogPostEntity));

        using (new AssertionScope())
        {
            entityType.Should().NotBeNull();
            entityType!.GetContainer().Should().Be(CosmosContainerConstants.MainContainer);
            entityType.FindDiscriminatorProperty()!.Name.Should().Be(nameof(BaseBeerEntity.EntityType));
            entityType.GetDiscriminatorValue().Should().Be(PartitionKeyConstants.NewsBlogPost);
            entityType.GetPartitionKeyPropertyNames().Should()
                .ContainInOrder(nameof(BaseBeerEntity.BrewerId), nameof(BaseBeerEntity.EntityType));
        }
    }

    private static BeersDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BeersDbContext>()
            .UseCosmos(
                accountEndpoint: "https://localhost:8081",
                accountKey: "AQIDBAUGBwgJCgsMDQ4PEA==",
                databaseName: "Beers")
            .Options;

        return new BeersDbContext(options);
    }
}
