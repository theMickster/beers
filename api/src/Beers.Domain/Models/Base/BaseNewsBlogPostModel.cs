namespace Beers.Domain.Models.Base;

public abstract class BaseNewsBlogPostModel
{
    public Guid BrewerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string PostType { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public List<string> ImageUrls { get; set; } = [];

    public DateTime? EventDate { get; set; }

    public string? EventLocation { get; set; }

    public DateTime? PublishedDate { get; set; }
}
