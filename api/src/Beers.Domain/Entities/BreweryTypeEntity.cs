using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BreweryTypeEntity : BaseMetaDataEntity
{
    public string TypeName = BeerMetadataPartitionKeyConstants.BreweryType;

    public override Guid TypeId { get; set; } = BeerMetadataPartitionKeyConstants.BreweryTypeGuid;
}
