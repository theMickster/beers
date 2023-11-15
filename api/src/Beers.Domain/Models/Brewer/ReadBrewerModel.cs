namespace Beers.Domain.Models.Brewer;

public sealed class ReadBrewerModel
{
    public Guid BrewerId { get; set; }

    public string Name { get; set; }
    
    public int FoundedIn { get; set; }

    public string Headquarters { get; set; }

    public string Website { get; set;  }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
