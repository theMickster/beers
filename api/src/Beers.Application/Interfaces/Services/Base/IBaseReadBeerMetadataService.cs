using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services.Base;

public interface IBaseReadBeerMetadataService<out T> where T : BaseMetaDataModel
{
    IReadOnlyList<T> GetList();
}
