using AutoMapper;
using Beers.Domain.Models.Brewer;

namespace Beers.Domain.Profiles;

public class BrewerReadModelToUpdateModel : Profile
{
    public BrewerReadModelToUpdateModel()
    {
        CreateMap<ReadBrewerModel, UpdateBrewerModel>()
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.Name,
                o => o.MapFrom(y => y.Name))
            .ForPath(x => x.FoundedIn,
                o => o.MapFrom(y => y.FoundedIn))
            .ForPath(x => x.Headquarters,
                o => o.MapFrom(y => y.Headquarters))
            .ForPath(x => x.Website,
                o => o.MapFrom(y => y.Website))
            .ForPath(x => x.BreweryType.Id,
                o => o.MapFrom(y => y.BreweryType.Id))
            .ForPath(x => x.BreweryType.Name,
                o => o.MapFrom(y => y.BreweryType.Name));
    }
}
