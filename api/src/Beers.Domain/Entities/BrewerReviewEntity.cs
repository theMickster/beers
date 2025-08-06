using Beers.Common.Constants;
using Beers.Domain.Entities.Base;

namespace Beers.Domain.Entities;

public sealed class BrewerReviewEntity : BaseBeerEntity
{
    public override string EntityType => PartitionKeyConstants.BrewerReview;

    public string ReviewerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public int Rating { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string ModifiedBy { get; set; } = string.Empty;
}
