﻿using AutoMapper;
using Beers.Application.Interfaces.Data;
using Beers.Application.Interfaces.Services.Base;
using Beers.Common.Settings;
using Beers.Domain.Entities.Base;
using Beers.Domain.Models.Base;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Beers.Application.Services.Base;

public abstract class ReadMetadataBaseService<TModel, TEntity>: IReadMetadataBaseService where TModel : MetadataBaseModel where TEntity : BaseMetaDataEntity
{
    protected readonly IBeersMetadataDbContext MetadataDbContext;
    protected readonly IMemoryCache MemoryCache;
    protected readonly IMapper Mapper;
    protected readonly IOptionsSnapshot<CacheSettings> CacheSettings;

    protected ReadMetadataBaseService(
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

    public async Task<List<T>> GetListAsync<T>()
    {
        MemoryCache.TryGetValue(CacheKey, out List<T>? cachedData);

        if (cachedData is { Count: > 0 })
        {
            return cachedData;
        }

        var entities = await GetEntitiesAsync();
        cachedData = Mapper.Map<List<T>>(entities);
        MemoryCache.Set(CacheKey, cachedData, TimeSpan.FromSeconds(CacheSettings.Value.TimeoutInSeconds));

        return cachedData;
    }


    protected abstract Task<List<TEntity>> GetEntitiesAsync();
}
