using System.Reflection;

namespace BeersDataLoader.Seeding;

internal sealed record SeedDataFilePaths(
    string BeerCategoryMetadataFile,
    string BeerStyleMetadataFile,
    string BeerTypeMetadataFile,
    string BreweryTypeMetadataFile,
    string BrewersDataFile,
    string BrewerReviewsDataFile,
    string BeersDataFile,
    string NewsBlogPostsDataFile
)
{
    internal static SeedDataFilePaths CreateFromAssemblyLocation()
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                       ?? throw new InvalidOperationException("Unable to determine loader assembly directory.");

        return new SeedDataFilePaths(
            BeerCategoryMetadataFile: Path.Combine(basePath, "Data", "BeerCategory.json"),
            BeerStyleMetadataFile: Path.Combine(basePath, "Data", "BeerStyle.json"),
            BeerTypeMetadataFile: Path.Combine(basePath, "Data", "BeerType.json"),
            BreweryTypeMetadataFile: Path.Combine(basePath, "Data", "BreweryType.json"),
            BrewersDataFile: Path.Combine(basePath, "Data", "Brewers.json"),
            BrewerReviewsDataFile: Path.Combine(basePath, "Data", "BrewerReviews.json"),
            BeersDataFile: Path.Combine(basePath, "Data", "Beers.json"),
            NewsBlogPostsDataFile: Path.Combine(basePath, "Data", "NewsBlogPosts.json"));
    }
}
