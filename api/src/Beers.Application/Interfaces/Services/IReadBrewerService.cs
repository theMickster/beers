using Beers.Domain.Models;
using Beers.Domain.Models.Brewer;

namespace Beers.Application.Interfaces.Services;

public interface IReadBrewerService
{
    Task<IReadOnlyList<ReadBrewerModel>> GetListAsync();

    Task<ReadBrewerModel?> GetByIdAsync(Guid brewerId);
}
