using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data;

public class BeersDbContext : DbContext, IBeersDbContext
{
    public BeersDbContext(DbContextOptions<BeersDbContext> options) : base(options)
    {
    }

    public DbSet<BrewerEntity> BrewerEntities { get; set; }

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
