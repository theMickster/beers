using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beers.Application.Data;

public class BeersMetadataDbContext : DbContext, IBeersMetadataDbContext
{
    public BeersMetadataDbContext(DbContextOptions<BeersMetadataDbContext> options) : base( options )
    {
    }

    public DbSet<BeerCategoryEntity> BeerCategories { get; set; }

    public DbSet<BeerStyleEntity> BeerStyles { get; set; }

    public DbSet<BeerTypeEntity> BeerTypes { get; set; }

    public DbSet<BreweryTypeEntity> BreweryTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(CosmosContainerConstants.MainContainer);

        var beerType = modelBuilder.Entity<BeerTypeEntity>();
        BeerMetadataConfiguring(beerType);
        beerType.HasDiscriminator(x => x.TypeName).HasValue(PartitionKeyConstants.BeerType);

        var beerStyle = modelBuilder.Entity<BeerStyleEntity>();
        BeerMetadataConfiguring(beerStyle);
        beerStyle.HasDiscriminator(x => x.TypeName).HasValue(PartitionKeyConstants.BeerStyle);
        
        var beerCategory = modelBuilder.Entity<BeerCategoryEntity>();
        BeerMetadataConfiguring(beerCategory);
        beerCategory.HasDiscriminator(x => x.TypeName).HasValue(PartitionKeyConstants.BeerCategory);

        var breweryTypes = modelBuilder.Entity<BreweryTypeEntity>();
        BeerMetadataConfiguring(breweryTypes);
        breweryTypes.HasDiscriminator(x => x.TypeName).HasValue(PartitionKeyConstants.BreweryType);
    }

    private static void BeerMetadataConfiguring<T>(EntityTypeBuilder<T> entityTypeBuilder) where T : BaseMetaDataEntity
    {
        entityTypeBuilder.Property(x => x.Id).ToJsonProperty("id");
        entityTypeBuilder.ToContainer(CosmosContainerConstants.MetadataContainer);
        entityTypeBuilder.HasPartitionKey(x => x.ApplicationName);
        entityTypeBuilder.HasPartitionKey(x => x.TypeId);
        entityTypeBuilder.HasKey(x => x.Id);
    }

}