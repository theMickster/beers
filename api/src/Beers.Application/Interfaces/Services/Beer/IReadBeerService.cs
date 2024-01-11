using Beers.Domain.Models.Beer;

namespace Beers.Application.Interfaces.Services.Beer;

public interface IReadBeerService
{
    /// <summary>
    /// Retrieve a list of beers
    /// </summary>
    /// <returns>A <see cref="List{T}"/> where T is a <see cref="ReadBeerModel"/></returns>
    Task<IReadOnlyList<ReadBeerModel>> GetListAsync();

    /// <summary>
    /// Retrieve a single beer by its unique identifier
    /// </summary>
    /// <param name="beerId">the unique identifier for a given record</param>
    /// <returns>A <see cref="ReadBeerModel"/> when the id matches, otherwise null</returns>
    Task<ReadBeerModel?> GetByIdAsync(Guid beerId);
}
