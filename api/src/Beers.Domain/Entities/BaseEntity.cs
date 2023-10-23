namespace Beers.Domain.Entities;

public abstract class BaseEntity
{
    /// <summary>
    /// A Cosmos Db entity primary identifier
    /// </summary>
    /// <example>5fe3fc2a-cbac-4df0-8031-fdca0f682989</example>
    public string Id { get; set; }
}
