using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BreweryTypeEntity : BaseMetaDataEntity
{
    public string TypeName = BeerPartitionKeyConstants.BreweryType;

    public override Guid TypeId { get; set; } = BeerPartitionKeyConstants.BreweryTypeGuid;
}
