using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Beer;

public interface IDeleteBeerService
{
    /// <summary>
    /// Performs process of deleting a beer.
    /// </summary>
    /// <param name="id">the beer id to delete</param>
    /// <returns></returns>
    Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid id);
}
