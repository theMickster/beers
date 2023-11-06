using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public sealed class BeerCategoryEntity : BaseMetaDataEntity
{
    public override string TypeName { get; set; } = BeerMetadataPartitionKeyConstants.BeerCategory;

    public override Guid TypeId { get; set; } = BeerMetadataPartitionKeyConstants.BeerCategoryGuid;
}
