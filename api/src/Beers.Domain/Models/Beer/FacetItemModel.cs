namespace Beers.Domain.Models.Beer;

public sealed class FacetItemModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Count { get; set; }
}
