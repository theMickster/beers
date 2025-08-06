using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Models.Brewer;

namespace Beers.Domain.Profiles;

public class BrewerReviewCreateModelToEntityProfile : Profile
{
    public BrewerReviewCreateModelToEntityProfile()
    {
        CreateMap<CreateBrewerReviewModel, BrewerReviewEntity>()
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.ReviewerName,
                o => o.MapFrom(y => y.ReviewerName))
            .ForPath(x => x.Title,
                o => o.MapFrom(y => y.Title))
            .ForPath(x => x.Comments,
                o => o.MapFrom(y => y.Comments))
            .ForPath(x => x.Rating,
                o => o.MapFrom(y => y.Rating));
    }
}
