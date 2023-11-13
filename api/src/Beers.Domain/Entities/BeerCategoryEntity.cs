using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public sealed class BeerCategoryEntity : BaseMetaDataEntity
{
    public string TypeName = BeerPartitionKeyConstants.BeerCategory;

    public override Guid TypeId { get; set; } = BeerPartitionKeyConstants.BeerCategoryGuid;
}
