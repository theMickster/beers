using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Enums;
using Beers.Domain.Models.NewsBlogPost;

namespace Beers.Domain.Profiles;

public sealed class NewsBlogPostCreateModelToEntityProfile : Profile
{
    public NewsBlogPostCreateModelToEntityProfile()
    {
        CreateMap<CreateNewsBlogPostModel, NewsBlogPostEntity>()
            .ForMember(x => x.Id,
                o => o.Ignore())
            .ForMember(x => x.EntityType,
                o => o.Ignore())
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.Title,
                o => o.MapFrom(y => y.Title))
            .ForPath(x => x.Body,
                o => o.MapFrom(y => y.Body))
            .ForPath(x => x.PostType,
                o => o.MapFrom(y => ParsePostType(y.PostType)))
            .ForPath(x => x.Tags,
                o => o.MapFrom(y => y.Tags))
            .ForPath(x => x.ImageUrls,
                o => o.MapFrom(y => y.ImageUrls))
            .ForPath(x => x.EventDate,
                o => o.MapFrom(y => y.EventDate))
            .ForPath(x => x.EventLocation,
                o => o.MapFrom(y => y.EventLocation))
            .ForPath(x => x.PublishedDate,
                o => o.MapFrom(y => y.PublishedDate))
            // TODO: Author must be populated by a future NewsBlogPostHydrationService (mirrors BeerHydrationService pattern).
            .ForMember(x => x.Author,
                o => o.Ignore())
            .ForMember(x => x.IsDeletable,
                o => o.Ignore());
    }

    private static NewsBlogPostType ParsePostType(string postType)
        => Enum.GetNames<NewsBlogPostType>().Contains(postType, StringComparer.OrdinalIgnoreCase)
            ? Enum.Parse<NewsBlogPostType>(postType, ignoreCase: true)
            : throw new ArgumentException($"Invalid post type: '{postType}'.", nameof(postType));
}
