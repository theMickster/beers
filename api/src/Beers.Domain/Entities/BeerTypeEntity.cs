using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BeerTypeEntity : BaseMetaDataEntity
{
    public override string TypeName { get; set; } = BeerMetadataPartitionKeyConstants.BeerType;
    
    public override Guid TypeId { get; set; } = BeerMetadataPartitionKeyConstants.BeerTypeGuid;
}