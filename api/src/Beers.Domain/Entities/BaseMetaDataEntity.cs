namespace Beers.Domain.Entities;

public abstract class BaseMetaDataEntity : BaseEntity
{
    public abstract Guid TypeId { get; }
}
