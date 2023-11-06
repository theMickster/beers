using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services;

public interface IReadBeerCategoryService
{
    IReadOnlyList<BeerCategoryModel> GetList();
}
