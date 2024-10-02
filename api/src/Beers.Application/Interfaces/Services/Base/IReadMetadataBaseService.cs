namespace Beers.Application.Interfaces.Services.Base;

public interface IReadMetadataBaseService
{
    Task<List<T>> GetListAsync<T>();
}
