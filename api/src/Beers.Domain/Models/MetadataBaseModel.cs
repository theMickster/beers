namespace Beers.Domain.Models;

public abstract class MetadataBaseModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
