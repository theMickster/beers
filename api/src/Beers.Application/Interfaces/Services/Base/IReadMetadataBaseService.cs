using Beers.Domain.Models;

namespace Beers.Application.Interfaces.Services.Base;

public interface IReadMetadataBaseService<out T> where T : MetadataBaseModel
{
    IReadOnlyList<T> GetList();
}
