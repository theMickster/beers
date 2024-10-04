using Newtonsoft.Json;

namespace BeersDataLoader.Entities;

internal class BreweryType
{
    [JsonProperty("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TypeId { get; set; } = PartitionKeyConstants.BreweryTypeGuid;

    public string TypeName = PartitionKeyConstants.BreweryType;

    public string Name { get; set; } = string.Empty;

    public Guid MetadataId { get; set; } = Guid.NewGuid();

    public string ApplicationName { get; set; } = "Beers";
}
