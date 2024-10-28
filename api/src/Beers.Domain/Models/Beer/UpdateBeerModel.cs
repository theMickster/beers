namespace Beers.Domain.Models.Beer;

public class UpdateBeerModel : CreateBeerModel
{
    public Guid BeerId { get; set; }
}