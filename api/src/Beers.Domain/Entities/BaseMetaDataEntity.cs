using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public abstract class BaseMetaDataEntity : BaseEntity
{
    public abstract Guid TypeId { get; set; }

    public abstract string TypeName { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MetadataId { get; set; }
}
