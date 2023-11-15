using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services;

public interface IReadBeerMetadataServiceBase<out T> where T : BaseMetaDataModel
{
    IReadOnlyList<T> GetList();
}
