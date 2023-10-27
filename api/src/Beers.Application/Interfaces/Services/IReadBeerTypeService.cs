using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services;

public interface IReadBeerTypeService
{
    IReadOnlyList<BeerTypeModel> GetList();
}
