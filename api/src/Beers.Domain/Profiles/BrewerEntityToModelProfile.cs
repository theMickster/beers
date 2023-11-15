using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Models;
using Beers.Domain.Models.Brewer;

namespace Beers.Domain.Profiles;

public class BrewerEntityToModelProfile : Profile
{
    public BrewerEntityToModelProfile()
    {
        CreateMap<BrewerEntity, ReadBrewerModel>()
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.Name,
                o => o.MapFrom(y => y.Name))
            .ForPath(x => x.Headquarters,
                o => o.MapFrom(y => y.Headquarters))
            .ForPath(x => x.Website,
                o => o.MapFrom(y => y.Website))
            .ForPath(x => x.FoundedIn,
                o => o.MapFrom(y => y.FoundedIn))
            .ForPath(x => x.CreatedDate,
                o => o.MapFrom(y => y.CreatedDate))
            .ForPath(x => x.ModifiedDate,
                o => o.MapFrom(y => y.ModifiedDate));
    }
}
