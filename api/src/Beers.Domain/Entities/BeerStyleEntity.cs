using Beers.Common.Constants;

namespace Beers.Domain.Entities;
public sealed class BeerStyleEntity : BaseMetaDataEntity
{
    public override string TypeName { get; set; } = BeerMetadataPartitionKeyConstants.BeerStyle;

    public override Guid TypeId { get; set; } = BeerMetadataPartitionKeyConstants.BeerStyleGuid;
}
