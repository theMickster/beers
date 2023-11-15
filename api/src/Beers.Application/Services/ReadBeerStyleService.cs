using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Application.Services.Base;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBeerStyleService : BaseReadBeerMetadataService<BeerStyleModel, BeerStyleEntity>, IReadBeerStyleService
{
    public ReadBeerStyleService(
        IMapper mapper,
        IBeersMetadataDbContext metadataDbContext,
        IMemoryCache memoryCache,
        IOptionsSnapshot<CacheSettings> cacheSettings)
        : base(CacheKeyConstants.BeerStyleList, mapper, metadataDbContext, memoryCache, cacheSettings) { }

    protected override IReadOnlyCollection<BeerStyleEntity> GetEntities()
    {
        return MetadataDbContext.BeerStyles.ToList().AsReadOnly();
    }
}
