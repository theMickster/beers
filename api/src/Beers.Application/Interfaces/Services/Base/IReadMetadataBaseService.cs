using Beers.Domain.Models;
using Beers.Domain.Models.Base;

namespace Beers.Application.Interfaces.Services.Base;

public interface IReadMetadataBaseService<out T> where T : MetadataBaseModel
{
    IReadOnlyList<T> GetList();
}
