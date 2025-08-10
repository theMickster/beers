using BeersDataLoader.Configuration;
using BeersDataLoader.Entities;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Seeding;
using BeersDataLoader.Seeding.Metadata;
using Microsoft.Azure.Cosmos;

Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("Welcome to the Cosmos Db data loader for the Beers project.");
Console.WriteLine($"Loader is executing in: {Environment.CurrentDirectory} ");

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("Please ensure that the following checklist is complete before continuing...");
Console.WriteLine("[✅] Configured an Environment Variables named 'BeersCosmosEndpoint' with a valid URL to a Cosmos Db account");
Console.WriteLine("[✅] Configured an Environment Variables named 'BeersCosmosKey' with a valid Cosmos Db account key");
Console.WriteLine("[✅] If using the local Cosmos Db Emulator, then you have started it with the /EnablePreview switch `.\\CosmosDB.Emulator.exe /EnablePreview` ");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Enter 'Y' to proceed or any other key to quit");
var response = Console.ReadKey(false).Key;
if (response != ConsoleKey.Y)
{
    Console.WriteLine("Exiting application");
    Environment.Exit(0);
}

Console.WriteLine();
Console.WriteLine("Proceeding with data loader...");

var configuration = LoaderConfigurationBuilder.Build();
var cosmosSettings = CosmosSettingsResolver.Resolve(configuration, LoaderConfigurationKeys.DefaultDatabaseName);
var seedDataFilePaths = SeedDataFilePaths.CreateFromAssemblyLocation();
var cosmosProvisioner = new CosmosProvisioner();
var metadataSeeder = new MetadataSeeder();
var reviewSeedGenerator = new BrewerReviewSeedGenerator();
var reviewSeeder = new BrewerReviewSeeder();

using CosmosClient cosmosClient = new(
    accountEndpoint: cosmosSettings.AccountEndpoint,
    authKeyOrResourceToken: cosmosSettings.SecurityKey
);

var database = await cosmosProvisioner.EnsureDatabaseAsync(cosmosClient, cosmosSettings.DatabaseName);
var metadataContainer = await cosmosProvisioner.EnsureMetadataContainerAsync(database, cosmosSettings.DatabaseName);
var beersContainer = await cosmosProvisioner.EnsureBeersContainerAsync(database, cosmosSettings.DatabaseName);

var result = await metadataSeeder.SeedAsync<BeerCategory>(metadataContainer, seedDataFilePaths.BeerCategoryMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Category Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");

result = await metadataSeeder.SeedAsync<BeerStyle>(metadataContainer, seedDataFilePaths.BeerStyleMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Style Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");

result = await metadataSeeder.SeedAsync<BeerType>(metadataContainer, seedDataFilePaths.BeerTypeMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Type Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");

result = await metadataSeeder.SeedAsync<BreweryType>(metadataContainer, seedDataFilePaths.BreweryTypeMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Brewery Type Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");

var reviewsGenerated = await reviewSeedGenerator.EnsureDataFileAsync(seedDataFilePaths.BrewersDataFile, seedDataFilePaths.BrewerReviewsDataFile);
if (!reviewsGenerated)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Unable to generate brewer review data file.");
}
else
{
    var reviewImportResult = await reviewSeeder.SeedAsync(beersContainer, seedDataFilePaths.BrewerReviewsDataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Brewer Review Import Total = {reviewImportResult.TotalItems} Created Total = {reviewImportResult.CreatedItems} Skipped Total {reviewImportResult.SkippedItems}");
}

Console.ReadLine();
