using Beers.Domain.Models.Beer;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Beer;

public interface ICreateBeerService
{
    /// <summary>
    /// Performs process of creating a new beer.
    /// </summary>
    /// <param name="inputModel">the new beer to create</param>
    /// <returns></returns>
    Task<(ReadBeerModel, List<ValidationFailure>)> CreateAsync(CreateBeerModel inputModel);
}
