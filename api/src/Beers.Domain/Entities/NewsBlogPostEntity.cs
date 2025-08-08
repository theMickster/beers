using Beers.Common.Constants;
using Beers.Domain.Entities.Base;
using Beers.Domain.Enums;

namespace Beers.Domain.Entities;

public sealed class NewsBlogPostEntity : BaseBeerEntity
{
    public override string EntityType => PartitionKeyConstants.NewsBlogPost;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NewsBlogPostType PostType { get; set; } = NewsBlogPostType.TextPost;

    public List<string> Tags { get; set; } = [];

    public List<string> ImageUrls { get; set; } = [];

    public DateTime? EventDate { get; set; }

    public string? EventLocation { get; set; }

    public DateTime? PublishedDate { get; set; }

    public BrewerSlimEntity Author { get; set; } = new();
}
