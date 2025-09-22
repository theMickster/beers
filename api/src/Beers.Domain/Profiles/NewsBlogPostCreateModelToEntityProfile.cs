using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Enums;
using Beers.Domain.Models.NewsBlogPost;

using System;
using System.Linq;

namespace Beers.Domain.Profiles;

/// <summary>
/// DEPRECATED: Mapping direct create-model to entity is unsafe. Prefer using the NewsBlogPostHydrationService to build
/// NewsBlogPostEntity instances. This profile is retained for compatibility but is marked obsolete to discourage use.
/// </summary>
[Obsolete("NewsBlogPostCreateModelToEntityProfile is deprecated. Use INewsBlogPostHydrationService.HydrateEntity instead.")]
public sealed class NewsBlogPostCreateModelToEntityProfile : Profile
{
    public NewsBlogPostCreateModelToEntityProfile()
    {
        // Retain mapping to avoid breaking existing code/tests while signaling deprecation.
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
            // NOTE: Author must be populated by NewsBlogPostHydrationService; do not map Author from client input.
            .ForMember(x => x.Author,
                o => o.Ignore())
            .ForMember(x => x.IsDeletable,
                o => o.Ignore())
            .ForMember(x => x.CreatedBy,
                o => o.Ignore())
            .ForMember(x => x.ModifiedBy,
                o => o.Ignore())
            .ForMember(x => x.CreatedDate,
                o => o.Ignore())
            .ForMember(x => x.ModifiedDate,
                o => o.Ignore());
    }

    private static NewsBlogPostType ParsePostType(string postType)
        => Enum.GetNames<NewsBlogPostType>().Contains(postType, StringComparer.OrdinalIgnoreCase)
            ? Enum.Parse<NewsBlogPostType>(postType, ignoreCase: true)
            : throw new ArgumentException($"Invalid post type: '{postType}'.", nameof(postType));
}
