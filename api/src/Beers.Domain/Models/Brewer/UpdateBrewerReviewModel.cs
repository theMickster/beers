using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Brewer;

public sealed class UpdateBrewerReviewModel : BaseBrewerReviewModel
{
    public Guid ReviewId { get; set; }
}
