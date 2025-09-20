using Beers.Domain.Entities;
using Beers.Domain.Models.NewsBlogPost;

namespace Beers.Application.Interfaces.Services.Hydration;

public interface INewsBlogPostHydrationService
{
    Task<NewsBlogPostEntity> HydrateEntity(CreateNewsBlogPostModel model);
}
