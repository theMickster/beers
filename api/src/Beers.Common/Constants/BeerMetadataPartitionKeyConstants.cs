﻿namespace Beers.Common.Constants;

public static class BeerMetadataPartitionKeyConstants
{
    public static readonly Guid BeerTypeGuid = new("752d4d23-548e-4d1a-9a41-badccbbd7dd9");

    public static readonly string BeerType = BeerTypeGuid.ToString();

    public static readonly Guid BeerStyleGuid = new("1253cac1-9950-42fd-9b8f-0a44ba7ce5ce");

    public static readonly string BeerStyle = BeerStyleGuid.ToString();

    public static readonly Guid BeerCategoryGuid = new("29d52320-4a29-4181-856e-562501e3f49c");

    public static readonly string BeerCategory = BeerCategoryGuid.ToString();
}
