using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Models.Metadata;
using Beers.Domain.Models.NewsBlogPost;

namespace Beers.Domain.Profiles;

public sealed class NewsBlogPostEntityToModelProfile : Profile
{
    public NewsBlogPostEntityToModelProfile()
    {
        CreateMap<NewsBlogPostEntity, ReadNewsBlogPostModel>()
            .ForPath(x => x.NewsBlogPostId,
                o => o.MapFrom(y => y.Id))
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.Title,
                o => o.MapFrom(y => y.Title))
            .ForPath(x => x.Body,
                o => o.MapFrom(y => y.Body))
            .ForPath(x => x.PostType,
                o => o.MapFrom(y => y.PostType.ToString()))
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
            .ForMember(x => x.Author,
                o => o.MapFrom(y => y.Author))
            .ForPath(x => x.IsDeletable,
                o => o.MapFrom(y => y.IsDeletable));

        CreateMap<BrewerSlimEntity, BrewerModel>()
            .ForMember(x => x.Id,
                o => o.MapFrom(y => y.Id));
    }
}
