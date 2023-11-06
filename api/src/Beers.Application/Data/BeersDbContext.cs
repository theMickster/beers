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

    public DbSet<BeerCategoryEntity> BeerCategories { get; set; }

    public DbSet<BeerStyleEntity> BeerStyles { get; set; }

    public DbSet<BeerTypeEntity> BeerTypes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(CosmosContainerConstants.MainContainer);

        BuildMetaDataEntity<BeerTypeEntity>(modelBuilder);

        BuildMetaDataEntity<BeerStyleEntity>(modelBuilder);

        BuildMetaDataEntity<BeerCategoryEntity>(modelBuilder);

        modelBuilder.Entity<BeerCategoryEntity>()
            .HasDiscriminator<string>(BeerMetadataPartitionKeyConstants.BeerCategory);

        modelBuilder.Entity<BeerCategoryEntity>()
            .HasPartitionKey(x => x.TypeName);

        modelBuilder.Entity<BeerStyleEntity>()
            .HasDiscriminator<string>(BeerMetadataPartitionKeyConstants.BeerStyle);

        modelBuilder.Entity<BeerStyleEntity>()
            .HasPartitionKey(x => x.TypeName);

        modelBuilder.Entity<BeerTypeEntity>()
            .HasDiscriminator<string>(BeerMetadataPartitionKeyConstants.BeerType);

        modelBuilder.Entity<BeerTypeEntity>()
            .HasPartitionKey(x => x.TypeName);


    }

    private static void BuildMetaDataEntity<T>(ModelBuilder modelBuilder) where T : BaseMetaDataEntity
    {
        modelBuilder.Entity<T>()
            .ToContainer(CosmosContainerConstants.MetadataContainer);

        modelBuilder.Entity<T>()
            .HasKey(x => new { x.Id });

        modelBuilder.Entity<T>()
            .Property(x => x.Id)
            .ToJsonProperty("id");

        modelBuilder.Entity<T>()
            .Property(x => x.TypeId);

        modelBuilder.Entity<T>()
            .Property(x => x.Name);

        modelBuilder.Entity<T>()
            .Property(x => x.TypeName);
    }
}