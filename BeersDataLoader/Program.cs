﻿using Microsoft.Azure.Cosmos;
using System.Net;
using Newtonsoft.Json;
using BeersDataLoader.Entities;
using System.Reflection;

Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("Welcome to the Cosmos Db data loader for the Beers project.");
Console.WriteLine($"Loader is executing in: {Environment.CurrentDirectory} ");

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("Please ensure that the following checklist is complete before continuing...");
Console.WriteLine("[ ] Configured an Environment Variables named 'BeersCosmosEndpoint' with a valid URL to a Cosmos Db account");
Console.WriteLine("[ ] Configured an Environment Variables named 'BeersCosmosKey' with a valid Cosmos Db account key");
Console.WriteLine("[ ] If using the local Cosmos Db Emulator, then you have started it with the /EnablePreview switch `.\\CosmosDB.Emulator.exe /EnablePreview` ");

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

const string databaseName = "PlatformDatabases";
const string metadataContainerName = "Metadata";
const string beersContainerName = "Beers";
var beerCategoryMetadataFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\BeerCategory.json");
var beerStyleMetadataFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\BeerStyle.json");
var beerTypeMetadataFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\BeerType.json");
var breweryTypeMetadataFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\BreweryType.json");

using CosmosClient cosmosClient = new(
    accountEndpoint: Environment.GetEnvironmentVariable("BeersCosmosEndpoint")!,
    authKeyOrResourceToken: Environment.GetEnvironmentVariable("BeersCosmosKey")!
);

var database = await BuildCosmosDatabase(cosmosClient);
var metadataContainer = await BuildMetadataContainer(database);
var beersContainer = await BuildBeersContainer(database);

var result = await ImportBeerCategoryMetadata(metadataContainer, beerCategoryMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Category Metadata Import Total == {result}");

result = await ImportBeerStyleMetadata(metadataContainer, beerStyleMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Style Metadata Import Total == {result}");

result = await ImportBeerTypeMetadata(metadataContainer, beerTypeMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Type Metadata Import Total == {result}");

result = await ImportBreweryTypeMetadata(metadataContainer, breweryTypeMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Brewery Type Metadata Import Total == {result}");


Console.ReadLine();

return;

async Task<Database> BuildCosmosDatabase(CosmosClient client)
{
    var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseName, 1000);
    switch (databaseResponse.StatusCode)
    {
        case HttpStatusCode.Created:
            Console.WriteLine($"A new Cosmos Database named '{databaseName}' was successfully created.");
            break;
        case HttpStatusCode.OK:
            Console.WriteLine(
                $"A Cosmos Database named '{databaseName}' already exists for the configured Cosmos Db instance.");
            break;
        default:
            throw new InvalidOperationException(
                $"Unable to create or locate a Cosmos Database named '{databaseName}'. Status Code: {databaseResponse.StatusCode}");
    }

    return databaseResponse.Database;
}

async Task<Container> BuildMetadataContainer(Database db)
{
    List<string> metadataPartitionKeys = ["/ApplicationName", "/TypeId"];
    var metadataContainerProperties = new ContainerProperties(metadataContainerName, partitionKeyPaths: metadataPartitionKeys);

    var container = await db.CreateContainerIfNotExistsAsync(metadataContainerProperties, 400);
    switch (container.StatusCode)
    {
        case HttpStatusCode.Created:
            Console.WriteLine($"A new Cosmos Container named '{metadataContainerName}' was successfully created for the Cosmos Db: '{databaseName}'.");
            break;
        case HttpStatusCode.OK:
            Console.WriteLine($"A Cosmos Container named '{metadataContainerName}' already exists for the Cosmos Db: '{databaseName}'.");
            break;
        default:
            throw new InvalidOperationException($"Unable to create or locate a Cosmos Container named '{metadataContainerName}'. Status Code: {container.StatusCode}");
    }
    return container;
}

async Task<Container> BuildBeersContainer(Database db)
{
    List<string> beersPartitionKeys = ["/BrewerId", "/EntityType"];
    var beerContainerProperties = new ContainerProperties(beersContainerName, partitionKeyPaths: beersPartitionKeys);

    var beerContainer = await db.CreateContainerIfNotExistsAsync(beerContainerProperties, 400);
    switch (beerContainer.StatusCode)
    {
        case HttpStatusCode.Created:
            Console.WriteLine($"A new Cosmos Container named '{beersContainerName}' was successfully created for the Cosmos Db: '{databaseName}'.");
            break;
        case HttpStatusCode.OK:
            Console.WriteLine($"A Cosmos Container named '{beersContainerName}' already exists for the Cosmos Db: '{databaseName}'.");
            break;
        default:
            throw new InvalidOperationException($"Unable to create or locate a Cosmos Container named '{beersContainerName}'. Status Code: {beerContainer.StatusCode}");
    }
    return beerContainer;
}

async Task<int> ImportBeerCategoryMetadata(Container container, string metadataFilePath)
{
    if (!File.Exists(metadataFilePath))
    {
        Console.WriteLine($"Invalid path to beer category metadata: {metadataFilePath}");
        return -1;
    }

    var json = await File.ReadAllTextAsync(metadataFilePath);
    var items = JsonConvert.DeserializeObject<List<BeerCategory>>(json) ?? [];

    return items.Count;
}

async Task<int> ImportBeerTypeMetadata(Container container, string metadataFilePath)
{
    if (!File.Exists(metadataFilePath))
    {
        Console.WriteLine($"Invalid path to beer type metadata: {metadataFilePath}");
        return -1;
    }

    var json = await File.ReadAllTextAsync(metadataFilePath);
    var items = JsonConvert.DeserializeObject<List<BeerType>>(json) ?? [];

    return items.Count;
}

async Task<int> ImportBeerStyleMetadata(Container container, string metadataFilePath)
{
    if (!File.Exists(metadataFilePath))
    {
        Console.WriteLine($"Invalid path to beer style metadata: {metadataFilePath}");
        return -1;
    }

    var json = await File.ReadAllTextAsync(metadataFilePath);
    var items = JsonConvert.DeserializeObject<List<BeerStyle>>(json) ?? [];

    return items.Count;
}

async Task<int> ImportBreweryTypeMetadata(Container container, string metadataFilePath)
{
    if (!File.Exists(metadataFilePath))
    {
        Console.WriteLine($"Invalid path to brewery type metadata: {metadataFilePath}");
        return -1;
    }

    var json = await File.ReadAllTextAsync(metadataFilePath);
    var items = JsonConvert.DeserializeObject<List<BreweryType>>(json) ?? [];

    return items.Count;
}


