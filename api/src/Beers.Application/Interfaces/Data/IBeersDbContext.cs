using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Beers.Application.Interfaces.Data;

public interface IBeersDbContext 
{
    DbSet<BrewerEntity> BrewerEntities { get; set; }

    DbSet<BeerEntity> BeerEntities { get; set; }

    Task<HttpStatusCode> AddBeerEntityAsync(BeerEntity entity);

    Task<HttpStatusCode> AddBreweryEntityAsync(BrewerEntity brewerEntity);

    Task<HttpStatusCode> UpdateBreweryEntityAsync(BrewerEntity brewerEntity);

    Task<HttpStatusCode> DeleteBreweryEntityAsync(Guid brewerId);
}
