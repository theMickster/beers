﻿namespace Beers.Domain.Entities;

public sealed class PriceEntity
{
    public decimal Price { get; set; }
    
    public int Quantity { get; set; }
    
    public int UnitVolume { get; set; }

    public string Packaging { get; set; } = string.Empty;
}
