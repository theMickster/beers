using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Attributes;
using Beers.Domain.Models.Beer;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services.Beer;

[ServiceLifetimeScoped]
public sealed class ReadBeerService(IMapper mapper, IDbContextFactory<BeersDbContext> dbContextFactory) 
    : IReadBeerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    /// <summary>
    /// Retrieve a list of beers
    /// </summary>
    /// <returns>A <see cref="List{T}"/> where T is a <see cref="ReadBeerModel"/></returns>
    public async Task<IReadOnlyList<ReadBeerModel>> GetListAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entities = await context.BeerEntities.ToListAsync();
        return _mapper.Map<List<ReadBeerModel>>(entities).AsReadOnly();
    }

    /// <summary>
    /// Retrieve a single beer by its unique identifier
    /// </summary>
    /// <param name="beerId">the unique identifier for a given record</param>
    /// <returns>A <see cref="ReadBeerModel"/> when the id matches, otherwise null</returns>
    public async Task<ReadBeerModel?> GetByIdAsync(Guid beerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entity = await context.BeerEntities.FirstOrDefaultAsync(x => x.Id == beerId);
        return entity == null ? null : _mapper.Map<ReadBeerModel>(entity);
    }
}
