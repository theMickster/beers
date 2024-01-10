using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Beer;

public class ReadBeerModel: BaseBeerModel
{
    public Guid BeerId { get; set; }
    
    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
