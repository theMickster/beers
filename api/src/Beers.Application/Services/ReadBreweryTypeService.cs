using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBreweryTypeService : ReadBeerMetadataServiceBase<BreweryTypeModel, BreweryTypeEntity>, IReadBreweryTypeService
{
    public ReadBreweryTypeService(
        IMapper mapper, 
        IBeersMetadataDbContext metadataDbContext, 
        IMemoryCache memoryCache,
        IOptionsSnapshot<CacheSettings> cacheSettings)
        : base(CacheKeyConstants.BreweryTypeList, mapper, metadataDbContext, memoryCache, cacheSettings) { }

    protected override IReadOnlyCollection<BreweryTypeEntity> GetEntities()
    {
        return MetadataDbContext.BreweryTypes.ToList().AsReadOnly();
    }
}
