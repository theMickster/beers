using Beers.Common.Constants;
using Beers.Domain.Entities.Base;

namespace Beers.Domain.Entities;

public class BeerTypeEntity : BaseMetaDataEntity
{
    public string TypeName = PartitionKeyConstants.BeerType;
    
    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerTypeGuid;
}