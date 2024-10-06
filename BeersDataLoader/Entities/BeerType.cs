namespace BeersDataLoader.Entities;

internal class BeerType : BaseMetadataEntity
{
    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerTypeGuid;

    public override string TypeName => PartitionKeyConstants.BeerType;
}
