﻿using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;

namespace Beers.Application.Data;

public class BeersDbContext(
    DbContextOptions<BeersDbContext> options,
    CosmosClient cosmosClient,
    IOptionsSnapshot<CosmosDbConnectionSettings> cosmosDbSettings,
    ILogger<BeersDbContext> logger)
    : DbContext(options), IBeersDbContext
{
    private readonly ILogger<BeersDbContext> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly CosmosClient _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
    private readonly IOptionsSnapshot<CosmosDbConnectionSettings> _cosmosDbSettings = cosmosDbSettings ?? throw new ArgumentNullException(nameof(cosmosDbSettings));

    public DbSet<BrewerEntity> BrewerEntities { get; set; } = null!;

    public DbSet<BeerEntity> BeerEntities { get; set; } = null!;

    #region Pubic Methods

    public async Task<HttpStatusCode> AddBreweryEntityAsync(BrewerEntity brewerEntity)
    {
        var container = _cosmosClient.GetDatabase(_cosmosDbSettings.Value.DatabaseName).GetContainer(CosmosContainerConstants.MainContainer);
        var result = await container.CreateItemAsync(brewerEntity);
        return result.StatusCode;
    }

    public async Task<HttpStatusCode> UpdateBreweryEntityAsync(BrewerEntity brewerEntity)
    {
        var container = _cosmosClient.GetDatabase(_cosmosDbSettings.Value.DatabaseName).GetContainer(CosmosContainerConstants.MainContainer);
        var partitionKey = new PartitionKeyBuilder().Add(brewerEntity.Id.ToString()).Add(PartitionKeyConstants.Brewer).Build();
        var result = await container.ReplaceItemAsync(brewerEntity, brewerEntity.Id.ToString(), partitionKey);
        return result.StatusCode;
    }

    public async Task<HttpStatusCode> DeleteBreweryEntityAsync(Guid brewerId)
    {
        var container = _cosmosClient.GetDatabase(_cosmosDbSettings.Value.DatabaseName).GetContainer(CosmosContainerConstants.MainContainer);
        var partitionKey = new PartitionKeyBuilder().Add(brewerId.ToString()).Add(PartitionKeyConstants.Brewer).Build();
        var result = await container.DeleteItemAsync<BrewerEntity>(brewerId.ToString(), partitionKey);
        return result.StatusCode;
    }

    #endregion Pubic Methods

    #region Protected Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.LogTo(message => Debug.WriteLine(message)).EnableSensitiveDataLogging().EnableDetailedErrors();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.HasDefaultContainer(CosmosContainerConstants.MainContainer);
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }

    #endregion Protected Methods
}
