using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services;

public interface IReadBeerStyleService
{
    IReadOnlyList<BeerStyleModel> GetList();
}
