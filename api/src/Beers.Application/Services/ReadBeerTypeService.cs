using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Application.Services.Base;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBeerTypeService : ReadMetadataBaseService<BeerTypeModel, BeerTypeEntity>, IReadBeerTypeService
{
    public ReadBeerTypeService(
        IMapper mapper,
        IBeersMetadataDbContext metadataDbContext,
        IMemoryCache memoryCache,
        IOptionsSnapshot<CacheSettings> cacheSettings)
        : base(CacheKeyConstants.BeerTypeList, mapper, metadataDbContext, memoryCache, cacheSettings) { }

    protected override async Task<List<BeerTypeEntity>> GetEntitiesAsync()
    {
        return await MetadataDbContext.BeerTypes.ToListAsync();
    }
}