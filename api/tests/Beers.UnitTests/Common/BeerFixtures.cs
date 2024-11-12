using Beers.Domain.Models.Brewer;

namespace Beers.UnitTests.Common;

internal static class BeerFixtures
{
    internal static Guid BrewerBootstrap = new("81ca37aa-e46f-4d0b-8b62-6eb7b5d6d240");
    internal static Guid BrewerFounders = new("fa25893a-5993-491b-bd22-a9ce1e61dbd0");
    internal static Guid BrewerNewBelgium = new ("13bf3bf9-91d6-482b-90bc-4d0bb8b57838");

    internal static List<ReadBrewerModel> GetBrewerModels() =>
    [
        new()
        {
            BrewerId = BrewerFounders, 
            Name = "Founders Brewing Company", 
            FoundedIn = 1997,
            Headquarters = "Grand Rapids, MI", 
            Website = "https://foundersbrewing.com", 
            IsDeletable = false,
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeRegional) 
        },
        new()
        {
            BrewerId = BrewerBootstrap,
            Name = "Bootstrap Brewing",
            FoundedIn = 2011,
            Headquarters = "Longmont, CO",
            Website = "https://bootstrapbrewing.com/",
            IsDeletable = false,
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeMicrobrewery)
        },
        new()
        {
            BrewerId = BrewerNewBelgium,
            Name = "New Belgium",
            FoundedIn = 1988,
            Headquarters = "Ft. Collins, Colorado",
            Website = "https://www.newbelgium.com/",
            IsDeletable = false,
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeLarge)
        },
    ];

}
