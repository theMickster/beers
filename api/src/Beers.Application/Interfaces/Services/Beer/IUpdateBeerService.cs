using Beers.Domain.Models.Beer;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Beer;

public interface IUpdateBeerService
{
    /// <summary>
    /// Performs process of updating a beer.
    /// </summary>
    /// <param name="inputModel">the beer to update</param>
    /// <returns></returns>
    Task<(ReadBeerModel, List<ValidationFailure>)> UpdateAsync(UpdateBeerModel inputModel);
}
