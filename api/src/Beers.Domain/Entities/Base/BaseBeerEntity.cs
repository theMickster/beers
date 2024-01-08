namespace Beers.Domain.Entities.Base;

public abstract class BaseBeerEntity : BaseEntity
{
    public Guid BrewerId { get; set; }
}
