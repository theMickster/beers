using Beers.Common.Settings;
using Beers.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Beers.Application.Interfaces.Data;

public interface IBeersDbContext 
{
    DbSet<BrewerEntity> BrewerEntities { get; set; }

    Task<HttpStatusCode> AddBreweryEntityAsync(CosmosClient cosmosDbClient, CosmosDbConnectionSettings cosmosDbSettings, BrewerEntity brewerEntity);
}
