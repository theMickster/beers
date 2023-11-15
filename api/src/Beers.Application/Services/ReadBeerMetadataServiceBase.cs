﻿using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services;

public abstract class ReadBeerMetadataServiceBase<TModel, TEntity>: IReadBeerMetadataServiceBase<TModel> where TModel : BaseMetaDataModel where TEntity : BaseMetaDataEntity
{
    protected readonly IBeersMetadataDbContext MetadataDbContext;
    protected readonly IMemoryCache MemoryCache;
    protected readonly IMapper Mapper;
    protected readonly IOptionsSnapshot<CacheSettings> CacheSettings;

    protected ReadBeerMetadataServiceBase(
        string cacheKey, 
        IMapper mapper, 
        IBeersMetadataDbContext metadataDbContext, 
        IMemoryCache memoryCache, 
        IOptionsSnapshot<CacheSettings> cacheSettings)
    {
        CacheKey = cacheKey ?? throw new ArgumentNullException(nameof(cacheKey));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        MetadataDbContext = metadataDbContext ?? throw new ArgumentNullException(nameof(metadataDbContext));
        MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        CacheSettings = cacheSettings ?? throw new ArgumentNullException(nameof(cacheSettings));
    }

    public string CacheKey { get; }

    public IReadOnlyList<TModel> GetList()
    {
        MemoryCache.TryGetValue(CacheKey, out IReadOnlyList<TModel>? cachedData);

        if (cachedData is { Count: > 0 })
        {
            return cachedData;
        }

        var entities = GetEntities();
        cachedData = Mapper.Map<List<TModel>>(entities);
        MemoryCache.Set(CacheKey, cachedData, TimeSpan.FromSeconds(CacheSettings.Value.TimeoutInSeconds));

        return cachedData;
    }

    protected abstract IReadOnlyCollection<TEntity> GetEntities();
}
