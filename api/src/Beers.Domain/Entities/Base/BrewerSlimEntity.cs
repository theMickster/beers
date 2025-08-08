namespace Beers.Domain.Entities.Base;

public sealed class BrewerSlimEntity
{
    /// <summary>Defaults to <see cref="Guid.Empty"/> as the unhydrated sentinel. Must be explicitly set to a valid brewer Id before persistence.</summary>
    public Guid Id { get; set; } = Guid.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string Website { get; set; } = string.Empty;
}
