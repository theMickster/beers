using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Data;

public class BeersDbContext : DbContext, IBeersDbContext
{
    public BeersDbContext(DbContextOptions<BeersDbContext> options) : base( options )
    {
    }

    public DbSet<BeerTypeEntity> BeerTypes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(CosmosContainerConstants.MainContainer);

        modelBuilder.Entity<BeerTypeEntity>()
            .ToContainer(CosmosContainerConstants.MetadataContainer);

        modelBuilder.Entity<BeerTypeEntity>()
            .HasNoDiscriminator();

        modelBuilder.Entity<BeerTypeEntity>()
            .HasPartitionKey(x => x.TypeId);

        modelBuilder.Entity<BeerTypeEntity>()
            .HasKey(x => new {x.Id, x.TypeId});

        modelBuilder.Entity<BeerTypeEntity>()
            .Property(x => x.Id)
            .ToJsonProperty("id");

        modelBuilder.Entity<BeerTypeEntity>()
            .Property(x => x.Name)
            .ToJsonProperty("name");

        modelBuilder.Entity<BeerTypeEntity>()
            .Property(x => x.TypeName)
            .ToJsonProperty("typeName");

        modelBuilder.Entity<BeerTypeEntity>()
            .Property(x => x.TypeId)
            .ToJsonProperty("typeId");
    }
}