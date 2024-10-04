﻿using Newtonsoft.Json;

namespace BeersDataLoader.Entities;

internal class BeerType
{
    [JsonProperty("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TypeId { get; set; } = PartitionKeyConstants.BeerTypeGuid;

    public string TypeName = PartitionKeyConstants.BeerType;

    public string Name { get; set; } = string.Empty;

    public Guid MetadataId { get; set; } = Guid.NewGuid();

    public string ApplicationName { get; set; } = "Beers";
}