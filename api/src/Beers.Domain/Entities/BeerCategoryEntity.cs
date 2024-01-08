using Beers.Common.Constants;
using Beers.Domain.Entities.Base;

namespace Beers.Domain.Entities;

public sealed class BeerCategoryEntity : BaseMetaDataEntity
{
    public string TypeName = PartitionKeyConstants.BeerCategory;

    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerCategoryGuid;
}
