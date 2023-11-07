using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public sealed class BeerCategoryEntity : BaseMetaDataEntity
{
    public string TypeName = BeerMetadataPartitionKeyConstants.BeerCategory;

    public override Guid TypeId { get; set; } = BeerMetadataPartitionKeyConstants.BeerCategoryGuid;
}
