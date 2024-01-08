namespace Beers.Domain.Models.Base;

public abstract class BaseBrewerModel
{
    public string Name { get; set; } = string.Empty;

    public int FoundedIn { get; set; }

    public string Headquarters { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public BreweryTypeModel BreweryType { get; set; }
}
