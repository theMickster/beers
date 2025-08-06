namespace Beers.Domain.Models.Base;

public abstract class BaseBrewerReviewModel
{
    public Guid BrewerId { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public int Rating { get; set; }

    public bool IsDeletable { get; set; } = true;
}
