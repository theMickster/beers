using Beers.Domain.Models.Metadata;

namespace Beers.UnitTests.Common;

internal static class MetadataFixtures
{
    internal static Guid BeerCategoryIdCraft = new ("7d82eca0-bc69-4d5c-bbdd-9b3799461264");
    internal static Guid BeerCategoryIdDomestic = new("750bddce-3abc-4585-9000-e045c8aba866");
    internal static Guid BeerCategoryFlavouredSpecialty = new("c8798e3d-6fa4-4e4c-9214-fe44cb70cb97");
    internal static Guid BeerCategoryImport = new("5d915ecf-ad8c-4874-82d8-38bd04e0a526");
    internal static Guid BeerCategoryInternational = new("033b2989-8c3d-4262-9f83-ba54985eb274");

    internal static Guid BeerStyleAmber = new("8b0c0ce2-5438-4481-92ae-4500a7b7f657");
    internal static Guid BeerStyleAmericanPaleAle = new("17272690-a85b-4581-b126-d0a4f17c324f");
    internal static Guid BeerStyleBlonde = new("0af8723b-7cb0-48b1-a29a-5777480095b5");
    internal static Guid BeerStyleCzechPilsner = new ("a5e0829a-4c01-4974-ae07-749b627a1b88");
    internal static Guid BeerStyleHazyIpa = new("1e65b3a8-6049-4d96-9f54-46a24e367182");
    internal static Guid BeerStyleImperial = new("1e65b3a8-6049-4d96-9f54-46a24e367182");
    internal static Guid BeerStyleIndiaPaleAle = new("1602c845-7cff-41bf-b96a-e1fbbe58ccdd");

    internal static Guid BeerTypeAle = new("9d6969cf-5873-43f1-9c49-f32ab6b586f5");
    internal static Guid BeerTypeLager = new("757e6b21-8556-4252-a38a-308f99400ddf");
    internal static Guid BeerTypeSpecialty = new("65617065-7b4a-4467-a412-9227449fcd60");

    internal static Guid BreweryTypeInPlanning = new("b03fcb6a-0722-4158-a494-524149dc2349");
    internal static Guid BreweryTypeBrewpub = new("c8720a14-9c23-4f46-9c2b-128351ccbff9");
    internal static Guid BreweryTypeContract = new("abaf83e7-9ed0-4e3c-a92c-c142ba425104");
    internal static Guid BreweryTypeLarge = new("1636e63e-990c-4585-8495-b02719162b47");
    internal static Guid BreweryTypeMicrobrewery = new("3ac7980d-0385-40e0-bc74-4fe796c0da19");
    internal static Guid BreweryTypeRegional = new("9d77afda-0d2e-4982-a7ea-c31567c5b486");
    internal static Guid BreweryTypeTaproom = new("d7b73d58-e913-40b8-9e06-f47d3b5f7bb3");

    internal static List<BeerCategoryModel> GetBeerCategoryModels() =>
    [
        new() {Id = BeerCategoryIdCraft, Name = "Craft"},
        new() {Id = BeerCategoryIdDomestic, Name = "Domestic"},
        new() {Id = BeerCategoryFlavouredSpecialty, Name = "Flavoured or Specialty"},
        new() {Id = BeerCategoryImport, Name = "Import"},
        new() {Id = BeerCategoryInternational, Name = "International"}
    ];

    internal static List<BeerStyleModel> GetBeerStyleModels() =>
    [
        new() {Id = BeerStyleAmber, Name = "Amber"},
        new() {Id = BeerStyleAmericanPaleAle, Name = "American Pale Ale"},
        new() {Id = BeerStyleBlonde, Name = "Blonde"},
        new() {Id = BeerStyleCzechPilsner, Name = "Czech Pilsner"},
        new() {Id = BeerStyleHazyIpa, Name = "Hazy IPA"},
        new() {Id = BeerStyleImperial, Name = "Imperial"},
        new() {Id = BeerStyleIndiaPaleAle, Name = "India Pale Ale"},
    ];

    internal static List<BeerTypeModel> GetBeerTypeModels() =>
    [
        new() {Id = BeerTypeAle, Name = "Ale"},
        new() {Id = BeerTypeLager, Name = "Lager"},
        new() {Id = BeerTypeSpecialty, Name = "Specialty or Hybrid"}
    ];

    internal static List<BreweryTypeModel> GetBreweryTypeModels() =>
    [
        new() {Id = BreweryTypeInPlanning, Name = "Brewery In Planning"},
        new() {Id = BreweryTypeBrewpub, Name = "Brewpub"},
        new() {Id = BreweryTypeContract, Name = "Contract"},
        new() {Id = BreweryTypeLarge, Name = "Large"},
        new() {Id = BreweryTypeMicrobrewery, Name = "Microbrewery"},
        new() {Id = BreweryTypeRegional, Name = "Regional"},
        new() {Id = BreweryTypeTaproom, Name = "Taproom"}
    ];
}
