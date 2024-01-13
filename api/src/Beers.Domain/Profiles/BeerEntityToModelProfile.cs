using AutoMapper;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Models.Beer;
using Beers.Domain.Models.Metadata;

namespace Beers.Domain.Profiles;

public class BeerEntityToModelProfile : Profile
{
    public BeerEntityToModelProfile()
    {
        CreateMap<BeerEntity, ReadBeerModel>()
            .ForPath(x => x.BeerId,
                o => o.MapFrom(y => y.Id))
            .ForPath(x => x.BrewerId,
                o => o.MapFrom(y => y.BrewerId))
            .ForPath(x => x.Name,
                o => o.MapFrom(y => y.Name))
            .ForPath(x => x.Description,
                o => o.MapFrom(y => y.Description))
            .ForPath(x => x.Image,
                o => o.MapFrom(y => y.Image))
            .ForPath(x => x.Sku,
                o => o.MapFrom(y => y.Sku))
            .ForPath(x => x.CreatedDate,
                o => o.MapFrom(y => y.CreatedDate))
            .ForPath(x => x.ModifiedDate,
                o => o.MapFrom(y => y.ModifiedDate))
            .ForMember(x => x.Pricing, 
                o => o.MapFrom(y => y.Pricing))
            .ForMember(x => x.Brewer,
                o => o.MapFrom(y => y.Brewer))
            .ReverseMap();

        CreateMap<PriceEntity, PriceModel>();
        CreateMap<BrewerSlimEntity, BrewerModel>();

    }
}
