using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Interfaces.Data;

public interface IBeersDbContext
{
    DbSet<BeerCategoryEntity> BeerCategories { get; set; }

    DbSet<BeerStyleEntity> BeerStyles { get; set; }

    DbSet<BeerTypeEntity> BeerTypes { get; set; }

    DbSet<BreweryTypeEntity> BreweryTypes { get; set; }

}
