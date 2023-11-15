﻿using Beers.Common.Constants;

namespace Beers.Domain.Entities;

public class BreweryTypeEntity : BaseMetaDataEntity
{
    public string TypeName = PartitionKeyConstants.BreweryType;

    public override Guid TypeId { get; set; } = PartitionKeyConstants.BreweryTypeGuid;
}
