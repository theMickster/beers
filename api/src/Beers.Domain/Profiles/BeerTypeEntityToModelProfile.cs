using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Models;

namespace Beers.Domain.Profiles;

public sealed class BeerTypeEntityToModelProfile : Profile
{
    public BeerTypeEntityToModelProfile()
    {
        CreateMap<BeerTypeEntity, BeerTypeModel>()
            .ForPath(x => x.Id,
                o => o.MapFrom(y => y.Id))
            .ForPath(x => x.Name,
                o => o.MapFrom(y => y.Name));
    }
}
