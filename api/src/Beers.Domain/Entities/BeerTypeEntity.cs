using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BeerTypeEntity : BaseMetaDataEntity
{
    public string TypeName = BeerMetadataPartitionKeyConstants.BeerType;
    
    public override Guid TypeId { get; set; } = BeerMetadataPartitionKeyConstants.BeerTypeGuid;
}