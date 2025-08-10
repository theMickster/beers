using BeersDataLoader.Entities;

namespace BeersDataLoader.Seeding.Metadata;

internal sealed record MetadataSeedManifest(Type MetadataType, string FilePath)
{
    internal static List<MetadataSeedManifest> Build(string basePath)
    {
        return
        [
            new(typeof(BeerCategory), Path.Combine(basePath, "Data", "BeerCategory.json")),
            new(typeof(BeerStyle), Path.Combine(basePath, "Data", "BeerStyle.json")),
            new(typeof(BeerType), Path.Combine(basePath, "Data", "BeerType.json")),
            new(typeof(BreweryType), Path.Combine(basePath, "Data", "BreweryType.json"))
        ];
    }
}
