using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Brewer;

public sealed class ReadBrewerReviewModel : BaseBrewerReviewModel
{
    public Guid ReviewId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
