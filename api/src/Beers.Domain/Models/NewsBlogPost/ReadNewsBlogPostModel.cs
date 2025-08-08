using Beers.Domain.Models.Base;
using Beers.Domain.Models.Metadata;

namespace Beers.Domain.Models.NewsBlogPost;

public sealed class ReadNewsBlogPostModel : BaseNewsBlogPostModel
{
    public Guid NewsBlogPostId { get; set; }

    public BrewerModel Author { get; set; } = new();

    public bool IsDeletable { get; set; } = true;
}
