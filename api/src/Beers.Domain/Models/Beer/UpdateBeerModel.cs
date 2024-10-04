namespace Beers.Domain.Models.Beer;

public sealed class UpdateBeerModel : CreateBeerModel
{
    public Guid BeerId { get; set; }
}