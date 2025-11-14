using Beers.Application.Data;
using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

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

    [Fact]
    public void ModelConfiguration_ShouldStorePostTypeAsString()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(NewsBlogPostEntity));
        var postTypeProperty = entityType?.FindProperty(nameof(NewsBlogPostEntity.PostType));

        using (new AssertionScope())
        {
            postTypeProperty.Should().NotBeNull();
            postTypeProperty?.GetTypeMapping().Converter.Should().NotBeNull();
            postTypeProperty?.GetTypeMapping().Converter?.ProviderClrType.Should().Be(typeof(string));
        }
    }

    [Fact]
    public void ModelConfiguration_ShouldMapAuthorOwnedTypeToExpectedJsonProperties()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(NewsBlogPostEntity));
        var authorNavigation = entityType?.FindNavigation(nameof(NewsBlogPostEntity.Author));
        var authorEntityType = authorNavigation?.TargetEntityType;
        var idProperty = authorEntityType?.FindProperty(nameof(BrewerSlimEntity.Id));
        var nameProperty = authorEntityType?.FindProperty(nameof(BrewerSlimEntity.Name));
        var websiteProperty = authorEntityType?.FindProperty(nameof(BrewerSlimEntity.Website));

        using (new AssertionScope())
        {
            authorNavigation.Should().NotBeNull();
            authorEntityType.Should().NotBeNull();
            authorEntityType!.IsOwned().Should().BeTrue();
            authorEntityType.GetContainingPropertyName().Should().Be("Author");
            idProperty.Should().NotBeNull("Author Id property should be configured");
            nameProperty.Should().NotBeNull("Author Name property should be configured");
            websiteProperty.Should().NotBeNull("Author Website property should be configured");
#pragma warning disable EF1001
            idProperty?.FindAnnotation(CosmosAnnotationNames.PropertyName)?.Value.Should().Be("id");
            nameProperty?.FindAnnotation(CosmosAnnotationNames.PropertyName)?.Value.Should().Be("name");
            websiteProperty?.FindAnnotation(CosmosAnnotationNames.PropertyName)?.Value.Should().Be("website");
#pragma warning restore EF1001
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
