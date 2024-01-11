﻿using Beers.Common.Constants;
using Beers.Domain.Entities.Base;
using Newtonsoft.Json;

namespace Beers.Domain.Entities;

public class BeerEntity : BaseBeerEntity
{
    public string EntityType = PartitionKeyConstants.Beer;
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    
    public List<PriceEntity> Pricing { get; set; }
    
    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
