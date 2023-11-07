namespace Beers.Domain.Entities;

public abstract class BaseMetaDataEntity : BaseEntity
{
    public abstract Guid TypeId { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public Guid MetadataId { get; set; } = Guid.NewGuid();
}
