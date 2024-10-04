using Newtonsoft.Json;

namespace BeersDataLoader.Entities;

internal class BeerCategory
{
    [JsonProperty("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TypeId { get; set; } = PartitionKeyConstants.BeerCategoryGuid;

    public string TypeName = PartitionKeyConstants.BeerCategory;

    public string Name { get; set; } = string.Empty;

    public Guid MetadataId { get; set; } = Guid.NewGuid();

    public string ApplicationName { get; set; } = "Beers";
}