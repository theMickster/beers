using Beers.Domain.Models.Beer;

namespace Beers.Application.Interfaces.Services.Beer;

public interface IReadBeerService
{
    Task<IReadOnlyList<ReadBeerModel>> GetListAsync();

    Task<ReadBeerModel?> GetByIdAsync(Guid beerId);
}
