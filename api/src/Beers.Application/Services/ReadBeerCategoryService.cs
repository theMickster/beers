using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBeerCategoryService : IReadBeerCategoryService
{
    private readonly IBeersMetadataDbContext _metadataDbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IMapper _mapper;

    public ReadBeerCategoryService(IMapper mapper, IBeersMetadataDbContext metadataDbContext, IMemoryCache memoryCache)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _metadataDbContext = metadataDbContext ?? throw new ArgumentNullException(nameof(metadataDbContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public IReadOnlyList<BeerCategoryModel> GetList()
    {
        const string cacheKey = "GetBeerCategoryList";

        _memoryCache.TryGetValue(cacheKey, out IReadOnlyList<BeerCategoryModel>? cachedData);

        if (cachedData is { Count: > 0 })
        {
            return cachedData;
        }

        var entities = _metadataDbContext.BeerCategories.ToList().AsReadOnly();
        cachedData = _mapper.Map<List<BeerCategoryModel>>(entities);
        _memoryCache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(5));

        return cachedData;
    }

}
