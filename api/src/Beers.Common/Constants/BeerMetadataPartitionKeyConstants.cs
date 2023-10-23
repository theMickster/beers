namespace Beers.Common.Constants;

public static class BeerMetadataPartitionKeyConstants
{
    public static Guid BeerTypeGuid = new("752d4d23-548e-4d1a-9a41-badccbbd7dd9");

    public static string BeerType = BeerTypeGuid.ToString();
}
