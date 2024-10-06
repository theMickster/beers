namespace BeersDataLoader.Entities;

internal class BeerStyle : BaseMetadataEntity
{
    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerStyleGuid;

    public override string TypeName => PartitionKeyConstants.BeerStyle;
}
