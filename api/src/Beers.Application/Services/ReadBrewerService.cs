using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Domain.Models;
using Beers.Domain.Models.Brewer;
using Microsoft.EntityFrameworkCore;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBrewerService : IReadBrewerService
{
    private readonly IMapper _mapper;
    private readonly IBeersDbContext _beerDbContext;

    public ReadBrewerService(IMapper mapper, IBeersDbContext beersDbContext)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _beerDbContext = beersDbContext ?? throw new ArgumentNullException(nameof(beersDbContext));
    }

    public async Task<IReadOnlyList<ReadBrewerModel>> GetListAsync()
    {
        var entities = await _beerDbContext.BrewerEntities.ToListAsync();
        return _mapper.Map<List<ReadBrewerModel>>(entities).AsReadOnly();
    }

    public Task<ReadBrewerModel> GetByIdAsync()
    {
        throw new NotImplementedException();
    }
}
