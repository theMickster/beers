using Beers.Domain.Entities;
using Beers.Domain.Models.Base;

namespace Beers.Application.Interfaces.Services.Hydration;

public interface INewsBlogPostHydrationService
{
    /// <summary>
    /// Hydrate a NewsBlogPostEntity from a base model.
    /// Works with both CreateNewsBlogPostModel and UpdateNewsBlogPostModel
    /// since both inherit from BaseNewsBlogPostModel and share identical properties.
    /// </summary>
    Task<NewsBlogPostEntity> HydrateEntity(BaseNewsBlogPostModel model);
}
