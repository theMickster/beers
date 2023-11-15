namespace Beers.Domain.Entities.Base;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
