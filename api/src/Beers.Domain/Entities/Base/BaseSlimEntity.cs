namespace Beers.Domain.Entities.Base;

public abstract class BaseSlimEntity
{
    public Guid MetadataId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
}
