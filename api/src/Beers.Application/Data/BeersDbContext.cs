using Beers.Application.Interfaces.Data;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Beers.Application.Data;

public class BeersDbContext(DbContextOptions<BeersDbContext> options): DbContext(options), IBeersDbContext
{
    public DbSet<BrewerEntity> BrewerEntities { get; set; } = null!;

    public DbSet<BeerEntity> BeerEntities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.LogTo(message => Debug.WriteLine(message)).EnableSensitiveDataLogging().EnableDetailedErrors();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.HasDefaultContainer(CosmosContainerConstants.MainContainer);
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}
