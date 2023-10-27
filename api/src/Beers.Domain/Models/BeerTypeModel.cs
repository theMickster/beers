namespace Beers.Domain.Models;

public sealed class BeerTypeModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
