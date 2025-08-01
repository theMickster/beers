namespace BeersDataLoader;

internal static class PartitionKeyConstants
{
    public static readonly Guid BeerTypeGuid = new("752d4d23-548e-4d1a-9a41-badccbbd7dd9");

    public static readonly string BeerTypeId = BeerTypeGuid.ToString();

    public static readonly string BeerType = "BeerType";

    public static readonly Guid BeerStyleGuid = new("1253cac1-9950-42fd-9b8f-0a44ba7ce5ce");

    public static readonly string BeerStyleId = BeerStyleGuid.ToString();

    public static readonly string BeerStyle = "BeerStyle";

    public static readonly Guid BeerCategoryGuid = new("29d52320-4a29-4181-856e-562501e3f49c");

    public static readonly string BeerCategoryId = BeerCategoryGuid.ToString();

    public static readonly string BeerCategory = "BeerCategory";

    public static readonly Guid BreweryTypeGuid = new("eaffbb38-ea95-44e9-bea2-3c9c5a04cc0f");

    public static readonly string BreweryTypeId = BreweryTypeGuid.ToString();

    public static readonly string BreweryType = "BreweryType";

    public static readonly string Brewer = "Brewer";

    public static readonly string Beer = "Beer";

}