using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BeerTypeEntity : BaseMetaDataEntity
{
    public string TypeName = BeerPartitionKeyConstants.BeerType;
    
    public override Guid TypeId { get; set; } = BeerPartitionKeyConstants.BeerTypeGuid;
}