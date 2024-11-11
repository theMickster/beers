using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Application.Services.Base;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services.Metadata;

[ServiceLifetimeScoped]
public sealed class ReadBreweryTypeService : ReadMetadataBaseService<BreweryTypeModel, BreweryTypeEntity>, IReadBreweryTypeService
{
    public ReadBreweryTypeService(
        IMapper mapper, 
        IBeersMetadataDbContext metadataDbContext, 
        IMemoryCache memoryCache,
        IOptionsSnapshot<CacheSettings> cacheSettings)
        : base(CacheKeyConstants.BreweryTypeList, mapper, metadataDbContext, memoryCache, cacheSettings) { }

    protected override async Task<List <BreweryTypeEntity>> GetEntitiesAsync()
    {
        return await MetadataDbContext.BreweryTypes.ToListAsync();
    }
}
