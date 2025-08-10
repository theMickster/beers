namespace BeersDataLoader.Models;

internal readonly record struct SeedResult(int TotalItems, int CreatedItems, int SkippedItems)
{
    internal static SeedResult Invalid => new(-1, 0, 0);
}
