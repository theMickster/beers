using Newtonsoft.Json;

namespace Beers.Domain.Entities.Base;

public abstract class BaseBeerEntity : BaseEntity
{
    
    public Guid BrewerId { get; set; }

    public virtual string EntityType { get; set; } = string.Empty;

    [JsonProperty("isDeletable")]
    public bool IsDeletable { get; set; } = true;
}
