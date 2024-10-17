namespace BeersDataLoader.Entities;

internal class BreweryType : BaseMetadataEntity
{

    public override Guid TypeId { get; set; } = PartitionKeyConstants.BreweryTypeGuid;

    public override string TypeName => PartitionKeyConstants.BreweryType;
}
