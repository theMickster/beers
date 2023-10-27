namespace Beers.Domain.Entities;

public class BeerTypeEntity : BaseMetaDataEntity
{
    public string Name { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public override Guid TypeId { get; } = new("752d4d23-548e-4d1a-9a41-badccbbd7dd9");
}