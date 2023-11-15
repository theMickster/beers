using Beers.Common.Constants;
using Beers.Domain.Entities.Base;

namespace Beers.Domain.Entities;
public sealed class BeerStyleEntity : BaseMetaDataEntity
{
    public string TypeName = PartitionKeyConstants.BeerStyle;

    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerStyleGuid;
}
