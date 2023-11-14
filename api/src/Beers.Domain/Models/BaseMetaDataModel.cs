namespace Beers.Domain.Models;

public abstract class BaseMetaDataModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
