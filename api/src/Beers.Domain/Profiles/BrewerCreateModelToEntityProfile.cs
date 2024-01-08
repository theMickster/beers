using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Models.Brewer;

namespace Beers.Domain.Profiles;

public class BrewerCreateModelToEntityProfile : Profile
{
    public BrewerCreateModelToEntityProfile()
    {
        CreateMap<CreateBrewerModel, BrewerEntity>()
            .ForPath(x => x.Name,
                o => o.MapFrom(y => y.Name))
            .ForPath(x => x.Headquarters,
                o => o.MapFrom(y => y.Headquarters))
            .ForPath(x => x.Website,
                o => o.MapFrom(y => y.Website))
            .ForPath(x => x.FoundedIn,
                o => o.MapFrom(y => y.FoundedIn))
            .ForPath(x => x.BreweryType.MetadataId,
                o => o.MapFrom(y => y.BreweryType.Id))
            .ForPath(x => x.BreweryType.Name,
                o => o.MapFrom(y => y.BreweryType.Name));
    }
}
