using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.NewsBlogPost;

public sealed class UpdateNewsBlogPostModel : BaseNewsBlogPostModel
{
    public Guid NewsBlogPostId { get; set; }
}
