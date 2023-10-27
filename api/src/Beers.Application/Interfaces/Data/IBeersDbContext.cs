using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Interfaces.Data;

public interface IBeersDbContext
{
    DbSet<BeerTypeEntity> BeerTypes { get; set; }
}
