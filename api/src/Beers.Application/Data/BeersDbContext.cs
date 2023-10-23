using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Data;

public class BeersDbContext : DbContext, IBeersDbContext
{
    public BeersDbContext( DbContextOptions<BeersDbContext> options ) : base( options )
    {
    }

    public DbSet<BeerTypeEntity> BeerTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultContainer(CosmosContainerConstants.MainContainer);

        builder.Entity<BeerTypeEntity>().ToContainer(CosmosContainerConstants.MetadataContainer);
    }
}