using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Interfaces.Data;

public interface IBeersDbContext 
{
    DbSet<BrewerEntity> BrewerEntities { get; set; }

    DbSet<BeerEntity> BeerEntities { get; set; }

    DbSet<BrewerReviewEntity> BrewerReviewEntities { get; set; }

    DbSet<NewsBlogPostEntity> NewsBlogPostEntities { get; set; }
}
