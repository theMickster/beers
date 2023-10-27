using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBeerTypeService : IReadBeerTypeService
{
    private readonly IBeersDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IMapper _mapper;

    public ReadBeerTypeService(IMapper mapper, IBeersDbContext dbContext, IMemoryCache memoryCache)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public IReadOnlyList<BeerTypeModel> GetList()
    {
        const string cacheKey = "GetBeerTypeList";

        _memoryCache.TryGetValue(cacheKey, out IReadOnlyList<BeerTypeModel>? cachedData);

        if (cachedData != null)
        {
            return cachedData;
        }

        var entities = _dbContext.BeerTypes.Where(x => x.TypeId == BeerMetadataPartitionKeyConstants.BeerTypeGuid).ToList().AsReadOnly();
        cachedData = _mapper.Map<List<BeerTypeModel>>(entities);
        _memoryCache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(5));

        return cachedData;
    }
}