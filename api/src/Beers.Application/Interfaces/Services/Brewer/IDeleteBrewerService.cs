using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Brewer;

public interface IDeleteBrewerService
{
    /// <summary>
    /// Performs process of deleting a brewer.
    /// </summary>
    /// <param name="id">the brewer id to delete</param>
    /// <returns></returns>
    Task<(bool, List<ValidationFailure>)> DeleteAsync(Guid id);
}
