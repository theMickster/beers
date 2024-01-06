using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics;
using System.Net;

namespace Beers.Application.Data;

public class BeersDbContext : DbContext, IBeersDbContext
{
    public BeersDbContext(DbContextOptions<BeersDbContext> options) : base(options)
    {
    }

    public DbSet<BrewerEntity> BrewerEntities { get; set; }

    public async Task<HttpStatusCode> AddBreweryEntityAsync(CosmosClient cosmosDbClient, CosmosDbConnectionSettings cosmosDbSettings, BrewerEntity brewerEntity)
    {
        var container = cosmosDbClient.GetDatabase(cosmosDbSettings.DatabaseName).GetContainer(CosmosContainerConstants.MainContainer);

        var createResponse = await container.CreateItemAsync(brewerEntity);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            // Log a special message here....
        }
        return createResponse.StatusCode;
    }

    public async Task<HttpStatusCode> UpdateBreweryEntityAsync(
        CosmosClient cosmosDbClient,
        CosmosDbConnectionSettings cosmosDbSettings, 
        BrewerEntity brewerEntity)
    {
        var container = cosmosDbClient.GetDatabase(cosmosDbSettings.DatabaseName).GetContainer(CosmosContainerConstants.MainContainer);
        var partitionKey = new PartitionKeyBuilder().Add(brewerEntity.Id.ToString()).Add(PartitionKeyConstants.Brewer).Build();
        var upsertResponse = await container.ReplaceItemAsync(brewerEntity, brewerEntity.Id.ToString(), partitionKey);

        return upsertResponse.StatusCode;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.LogTo(message => Debug.WriteLine(message)).EnableSensitiveDataLogging().EnableDetailedErrors();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(CosmosContainerConstants.MainContainer);

        var brewerEntity = modelBuilder.Entity<BrewerEntity>();
        BeerConfiguring(brewerEntity);
        brewerEntity.HasDiscriminator(x => x.EntityType).HasValue(PartitionKeyConstants.Brewer);
        brewerEntity.OwnsOne(x => x.BreweryType);
    }
    
    private static void BeerConfiguring<T>(EntityTypeBuilder<T> entityTypeBuilder) where T : BaseBeerEntity
    {
        entityTypeBuilder.Property(x => x.Id).ToJsonProperty("id");
        entityTypeBuilder.ToContainer(CosmosContainerConstants.MainContainer);
        entityTypeBuilder.HasPartitionKey(x => x.BrewerId);
        entityTypeBuilder.HasKey(x => x.Id);
    }
}
