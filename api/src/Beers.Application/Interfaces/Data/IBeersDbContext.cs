using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Interfaces.Data;

public interface IBeersDbContext 
{
    DbSet<BrewerEntity> BrewerEntities { get; set; }

    DbSet<BeerEntity> BeerEntities { get; set; }
}
