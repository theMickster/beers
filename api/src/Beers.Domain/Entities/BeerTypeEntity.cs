using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BeerTypeEntity : BaseMetaDataEntity
{
    public string TypeName = PartitionKeyConstants.BeerType;
    
    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerTypeGuid;
}