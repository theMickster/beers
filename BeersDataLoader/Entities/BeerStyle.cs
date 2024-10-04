using Newtonsoft.Json;

namespace BeersDataLoader.Entities;

internal class BeerStyle
{
    [JsonProperty("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TypeId { get; set; } = PartitionKeyConstants.BeerStyleGuid;

    public string TypeName = PartitionKeyConstants.BeerStyle;

    public string Name { get; set; } = string.Empty;

    public Guid MetadataId { get; set; } = Guid.NewGuid();

    public string ApplicationName { get; set; } = "Beers";
}
