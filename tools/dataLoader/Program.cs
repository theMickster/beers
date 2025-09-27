using Azure.Identity;
using BeersDataLoader.Configuration;
using BeersDataLoader.Entities;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using BeersDataLoader.Seeding;
using Microsoft.Azure.Cosmos;

Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("Welcome to the Cosmos Db data loader for the Beers project.");
Console.WriteLine($"Loader is executing in: {Environment.CurrentDirectory} ");

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("Please ensure that the following checklist is complete before continuing...");
Console.WriteLine("[✅] Signed in with 'az login' before starting the loader");
Console.WriteLine($"[✅] Exported {LoaderConfigurationKeys.EndpointEnvironmentVariableName}");
Console.WriteLine($"[✅] Exported {LoaderConfigurationKeys.DatabaseEnvironmentVariableName}");
Console.WriteLine($"[✅] Exported {LoaderConfigurationKeys.BeersContainerEnvironmentVariableName}");
Console.WriteLine($"[✅] Exported {LoaderConfigurationKeys.MetadataContainerEnvironmentVariableName}");

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Enter 'Y' to proceed or any other key to quit");
var response = Console.ReadKey(false).Key;
if (response != ConsoleKey.Y)
{
    Console.WriteLine("Exiting application");
    Environment.Exit(0);
}

Console.WriteLine();
Console.WriteLine("Proceeding with data loader...");

var cosmosSettings = CosmosSettingsResolver.Resolve();
var seedDataFilePaths = SeedDataFilePaths.CreateFromAssemblyLocation();

using CosmosClient cosmosClient = new(
    accountEndpoint: cosmosSettings.AccountEndpoint,
    tokenCredential: new DefaultAzureCredential()
);

var database = await CosmosProvisioner.EnsureDatabaseAsync(cosmosClient, cosmosSettings.DatabaseName);
var metadataContainer = await CosmosProvisioner.EnsureMetadataContainerAsync(database, cosmosSettings.DatabaseName, cosmosSettings.MetadataContainerName);
var beersContainer = await CosmosProvisioner.EnsureBeersContainerAsync(database, cosmosSettings.DatabaseName, cosmosSettings.BeersContainerName);

await SeedBeerCategory(seedDataFilePaths, metadataContainer);
await SeedBeerStyle(seedDataFilePaths, metadataContainer);
await SeedBeerType(seedDataFilePaths, metadataContainer);
await SeedBreweryType(seedDataFilePaths, metadataContainer);
await SeedBrewer(seedDataFilePaths, beersContainer);

Console.ReadLine();

static async Task SeedBeerCategory(SeedDataFilePaths seedDataFilePaths, Container metadataContainer)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Beer Category Metadata Import Starting...");
    var result = await MetadataSeeder.SeedAsync<BeerCategory>(metadataContainer, seedDataFilePaths.BeerCategoryMetadataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Beer Category Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");
}

static async Task SeedBeerStyle(SeedDataFilePaths seedDataFilePaths, Container metadataContainer)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Beer Style Metadata Import Starting...");
    var result = await MetadataSeeder.SeedAsync<BeerStyle>(metadataContainer, seedDataFilePaths.BeerStyleMetadataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Beer Style Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");
}

static async Task SeedBeerType(SeedDataFilePaths seedDataFilePaths, Container metadataContainer)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Beer Type Metadata Import Starting...");
    var result = await MetadataSeeder.SeedAsync<BeerType>(metadataContainer, seedDataFilePaths.BeerTypeMetadataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Beer Type Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");
}

static async Task SeedBreweryType(SeedDataFilePaths seedDataFilePaths, Container metadataContainer)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Brewery Type Metadata Import Starting...");
    var result = await MetadataSeeder.SeedAsync<BreweryType>(metadataContainer, seedDataFilePaths.BreweryTypeMetadataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Brewery Type Metadata Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");
}

static async Task SeedBrewer(SeedDataFilePaths seedDataFilePaths, Container beersContainer)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Brewer Import Starting...");
    var result = await BrewerSeeder.SeedAsync(beersContainer, seedDataFilePaths.BrewersDataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Brewer Import Total = {result.TotalItems} Created Total = {result.CreatedItems} Skipped Total {result.SkippedItems}");
}