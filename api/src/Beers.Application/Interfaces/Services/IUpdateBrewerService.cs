using Beers.Domain.Models.Brewer;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services;

public interface IUpdateBrewerService
{
    /// <summary>
    /// Performs process of updating a brewer.
    /// </summary>
    /// <param name="inputModel">the brewer to update</param>
    /// <returns></returns>
    Task<(ReadBrewerModel, List<ValidationFailure>)> UpdateAsync(UpdateBrewerModel inputModel);
}
