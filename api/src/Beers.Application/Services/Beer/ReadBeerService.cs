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


    public async Task<IReadOnlyList<ReadBeerModel>> GetListAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entities = await context.BeerEntities.ToListAsync();
        return _mapper.Map<List<ReadBeerModel>>(entities).AsReadOnly();
    }

    public async Task<ReadBeerModel?> GetByIdAsync(Guid beerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entity = await context.BeerEntities.FirstOrDefaultAsync(x => x.Id == beerId);
        return entity == null ? null : _mapper.Map<ReadBeerModel>(entity);
    }
}
