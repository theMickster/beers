using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Models.Brewer;

namespace Beers.Domain.Profiles;

public class BrewerReviewEntityToModelProfile : Profile
{
    public BrewerReviewEntityToModelProfile()
    {
        CreateMap<BrewerReviewEntity, ReadBrewerReviewModel>()
            .ForPath(x => x.ReviewId,
                o => o.MapFrom(y => y.Id))
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.ReviewerName,
                o => o.MapFrom(y => y.ReviewerName))
            .ForPath(x => x.Title,
                o => o.MapFrom(y => y.Title))
            .ForPath(x => x.Comments,
                o => o.MapFrom(y => y.Comments))
            .ForPath(x => x.Rating,
                o => o.MapFrom(y => y.Rating))
            .ForPath(x => x.IsDeletable,
                o => o.MapFrom(y => y.IsDeletable))
            .ForPath(x => x.CreatedDate,
                o => o.MapFrom(y => y.CreatedDate))
            .ForPath(x => x.ModifiedDate,
                o => o.MapFrom(y => y.ModifiedDate))
            .ReverseMap();
    }
}
