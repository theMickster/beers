namespace BeersDataLoader.Entities;

internal class BeerCategory : BaseMetadataEntity
{
    public override Guid TypeId { get; set; } = PartitionKeyConstants.BeerCategoryGuid;

    public override string TypeName => PartitionKeyConstants.BeerCategory;
}