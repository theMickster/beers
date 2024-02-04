﻿using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Application.Services.Base;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models;
using Beers.Domain.Models.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services;

[ServiceLifetimeScoped]
public sealed class ReadBeerCategoryService : ReadMetadataBaseService<BeerCategoryModel, BeerCategoryEntity>, IReadBeerCategoryService
{
    public ReadBeerCategoryService (
        IMapper mapper,
        IBeersMetadataDbContext metadataDbContext,
        IMemoryCache memoryCache,
        IOptionsSnapshot<CacheSettings> cacheSettings)
        : base(CacheKeyConstants.BeerCategoryList, mapper, metadataDbContext, memoryCache, cacheSettings) { }

    protected override IReadOnlyCollection<BeerCategoryEntity> GetEntities()
    {
        return MetadataDbContext.BeerCategories.ToList().AsReadOnly();
    }
}
