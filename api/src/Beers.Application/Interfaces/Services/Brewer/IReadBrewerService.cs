using Beers.Domain.Models.Brewer;

namespace Beers.Application.Interfaces.Services.Brewer;

public interface IReadBrewerService
{
    Task<IReadOnlyList<ReadBrewerModel>> GetListAsync();

    Task<ReadBrewerModel?> GetByIdAsync(Guid brewerId);
}
