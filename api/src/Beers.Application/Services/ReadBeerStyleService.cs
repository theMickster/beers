using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBeerStyleService : IReadBeerStyleService
{
    private readonly IBeersDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IMapper _mapper;

    public ReadBeerStyleService(IMapper mapper, IBeersDbContext dbContext, IMemoryCache memoryCache)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public IReadOnlyList<BeerStyleModel> GetList()
    {
        const string cacheKey = "GetBeerStyleList";

        _memoryCache.TryGetValue(cacheKey, out IReadOnlyList<BeerStyleModel>? cachedData);

        if (cachedData != null)
        {
            return cachedData;
        }

        var entities = _dbContext.BeerStyles.ToList().AsReadOnly();
        cachedData = _mapper.Map<List<BeerStyleModel>>(entities);
        _memoryCache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(5));

        return cachedData;
    }
}
