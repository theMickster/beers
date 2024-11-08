using Beers.Domain.Models.Brewer;
using FluentValidation.Results;

namespace Beers.Application.Interfaces.Services.Brewer;

public interface ICreateBrewerService
{
    /// <summary>
    /// Performs process of creating a new brewer.
    /// </summary>
    /// <param name="inputModel">the new brewer to create</param>
    /// <returns></returns>
    Task<(ReadBrewerModel, List<ValidationFailure>)> CreateAsync(CreateBrewerModel inputModel);
}
