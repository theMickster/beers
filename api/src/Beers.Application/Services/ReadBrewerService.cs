using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Domain.Models.Brewer;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBrewerService(IMapper mapper, IDbContextFactory<BeersDbContext> dbContextFactory)
    : IReadBrewerService
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbContextFactory<BeersDbContext> _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

    public async Task<IReadOnlyList<ReadBrewerModel>> GetListAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entities = await context.BrewerEntities.ToListAsync();
        return _mapper.Map<List<ReadBrewerModel>>(entities).AsReadOnly();
    }

    public async Task<ReadBrewerModel?> GetByIdAsync(Guid brewerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var entity = await context.BrewerEntities.FirstOrDefaultAsync(x => x.Id == brewerId);
        return entity == null ? null : _mapper.Map<ReadBrewerModel>(entity);
    }
}
