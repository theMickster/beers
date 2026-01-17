using Beers.Common.Filtering.Beer;
using Beers.Domain.Models.Beer;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Beer;

public interface IDiscoverBeerService
{
    /// <summary>
    /// Filter beers and return the matching page along with facet counts.
    /// </summary>
    /// <param name="parameters">The paging and sorting parameters</param>
    /// <param name="input">The filters to apply</param>
    /// <returns>The faceted result, plus any validation failures. The result is empty when there are failures.</returns>
    Task<(FacetedSearchResultModel, List<ValidationFailure>)> DiscoverAsync(DiscoverBeerParameter parameters, FacetedSearchInputModel input);
}
