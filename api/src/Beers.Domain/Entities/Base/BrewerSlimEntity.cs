using Newtonsoft.Json;

namespace Beers.Domain.Entities.Base;

public sealed class BrewerSlimEntity
{
    [JsonProperty("id")]
    public Guid BrewerId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    
    public string Website { get; set; } = string.Empty;
}
