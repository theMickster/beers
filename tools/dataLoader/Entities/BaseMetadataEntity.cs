using Newtonsoft.Json;

namespace BeersDataLoader.Entities;

internal class BaseMetadataEntity
{
    [JsonProperty("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    public virtual Guid TypeId { get; set; } = Guid.NewGuid();

    public virtual string TypeName => string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid MetadataId { get; set; } = Guid.NewGuid();

    public string ApplicationName { get; set; } = PartitionKeyConstants.Beer;
}
