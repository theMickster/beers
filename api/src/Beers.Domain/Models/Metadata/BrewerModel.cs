﻿namespace Beers.Domain.Models.Metadata;

public sealed class BrewerModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;
}
