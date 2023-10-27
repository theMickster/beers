using System.Text.Json.Serialization;

namespace Beers.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
