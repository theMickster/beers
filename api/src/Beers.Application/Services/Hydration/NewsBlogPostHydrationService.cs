using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Common.Attributes;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Models.Base;

namespace Beers.Application.Services.Hydration;

[ServiceLifetimeScoped]
public sealed class NewsBlogPostHydrationService(
    IReadBrewerService readBrewerService) : INewsBlogPostHydrationService
{
    private readonly IReadBrewerService _readBrewerService = readBrewerService ?? throw new ArgumentNullException(nameof(readBrewerService));

    /// <summary>
    /// Hydrate a NewsBlogPostEntity from a base model.
    /// This single method handles both Create and Update workflows since both models
    /// inherit from BaseNewsBlogPostModel and share identical properties.
    /// </summary>
    public async Task<NewsBlogPostEntity> HydrateEntity(BaseNewsBlogPostModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var brewer = await _readBrewerService.GetByIdAsync(model.BrewerId).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(brewer);

        var entity = new NewsBlogPostEntity
        {
            BrewerId = model.BrewerId,
            Title = model.Title,
            Body = model.Body,
            PostType = Enum.Parse<Beers.Domain.Enums.NewsBlogPostType>(model.PostType, ignoreCase: true),
            Tags = model.Tags ?? new List<string>(),
            ImageUrls = model.ImageUrls ?? new List<string>(),
            EventDate = model.EventDate,
            EventLocation = model.EventLocation,
            PublishedDate = model.PublishedDate,
            Author = new BrewerSlimEntity
            {
                Id = model.BrewerId,
                Name = brewer.Name,
                Website = brewer.Website
            }
        };

        return entity;
    }
}
