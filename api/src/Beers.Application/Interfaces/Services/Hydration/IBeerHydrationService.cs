using Beers.Domain.Entities;
using Beers.Domain.Models.Beer;

namespace Beers.Application.Interfaces.Services.Hydration;

public interface IBeerHydrationService
{
    Task<BeerEntity> HydrateEntity(CreateBeerModel model);
}
