using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.NewsBlogPost;

public sealed class SearchInputNewsBlogPostModel : SearchInputModelBase
{
    public Guid? BrewerId { get; set; }

    public string PostType { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;

    public DateTime? DateRangeStart { get; set; }

    public DateTime? DateRangeEnd { get; set; }

    // When true, draft posts (PublishedDate == null) are included in results.
    // Auth gate for this flag is a future concern — no auth layer exists in this project yet.
    public bool IncludeDrafts { get; set; } = false;
}
