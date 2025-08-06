using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

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

const string databaseName = "PlatformDatabases";
const string metadataContainerName = "Metadata";
const string beersContainerName = "Beers";
var dataBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                   ?? throw new InvalidOperationException("Unable to determine loader assembly directory.");
var beerCategoryMetadataFile = Path.Combine(dataBasePath, "Data", "BeerCategory.json");
var beerStyleMetadataFile = Path.Combine(dataBasePath, "Data", "BeerStyle.json");
var beerTypeMetadataFile = Path.Combine(dataBasePath, "Data", "BeerType.json");
var breweryTypeMetadataFile = Path.Combine(dataBasePath, "Data", "BreweryType.json");
var brewersDataFile = Path.Combine(dataBasePath, "Data", "Brewers.json");
var brewerReviewsDataFile = Path.Combine(dataBasePath, "Data", "BrewerReviews.json");

using CosmosClient cosmosClient = new(
    accountEndpoint: Environment.GetEnvironmentVariable("BeersCosmosEndpoint")!,
    authKeyOrResourceToken: Environment.GetEnvironmentVariable("BeersCosmosKey")!
);

var database = await BuildCosmosDatabase(cosmosClient);
var metadataContainer = await BuildMetadataContainer(database);
var beersContainer = await BuildBeersContainer(database);

var result = await ImportBeerMetadata<BeerCategory>(metadataContainer, beerCategoryMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Category Metadata Import Total = {result.totalItems} Created Total = {result.createdItems} Skipped Total {result.skippedItems}");

result = await ImportBeerMetadata<BeerStyle>(metadataContainer, beerStyleMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Style Metadata Import Total = {result.totalItems} Created Total = {result.createdItems} Skipped Total {result.skippedItems}");

result = await ImportBeerMetadata<BeerType>(metadataContainer, beerTypeMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Beer Type Metadata Import Total = {result.totalItems} Created Total = {result.createdItems} Skipped Total {result.skippedItems}");

result = await ImportBeerMetadata<BreweryType>(metadataContainer, breweryTypeMetadataFile);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Brewery Type Metadata Import Total = {result.totalItems} Created Total = {result.createdItems} Skipped Total {result.skippedItems}");

var reviewsGenerated = await EnsureBrewerReviewDataFile(brewersDataFile, brewerReviewsDataFile);
if (!reviewsGenerated)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Unable to generate brewer review data file.");
}
else
{
    var reviewImportResult = await ImportBeerEntities(beersContainer, brewerReviewsDataFile);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Brewer Review Import Total = {reviewImportResult.totalItems} Created Total = {reviewImportResult.createdItems} Skipped Total {reviewImportResult.skippedItems}");
}

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

async Task<(int totalItems, int createdItems, int skippedItems)> ImportBeerMetadata<T>(Container container, string metadataFilePath) where T : BaseMetadataEntity
{
    if (!File.Exists(metadataFilePath))
    {
        Console.WriteLine($"Invalid path to {typeof(T)} metadata: {metadataFilePath}");
        return (-1, 0, 0);
    }

    var json = await File.ReadAllTextAsync(metadataFilePath);
    var items = JsonConvert.DeserializeObject<List<T>>(json) ?? [];
    var createdItems = 0;
    var skippedItems = 0;

    foreach (var item in items)
    {
        var createItem = false;
        var partitionKey = new PartitionKeyBuilder()
            .Add(item.ApplicationName)
            .Add(item.TypeId.ToString().ToLowerInvariant())
            .Build();

        try
        {
            await container.ReadItemAsync<T>(item.Id.ToString().ToLowerInvariant(), partitionKey);

        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            createItem = true;
        }

        if (!createItem)
        {
            skippedItems++;
            continue;
        }

        var createResponse = await container.CreateItemAsync(item, partitionKey);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            Console.WriteLine($"Failed to create {typeof(T)} : {item.Name}");
        }
        else
        {
            createdItems++;
        }
    }

    return (items.Count, createdItems, skippedItems);
}

async Task<(int totalItems, int createdItems, int skippedItems)> ImportBeerEntities(Container container, string dataFilePath)
{
    if (!File.Exists(dataFilePath))
    {
        Console.WriteLine($"Invalid path to beer entities data: {dataFilePath}");
        return (-1, 0, 0);
    }

    var json = await File.ReadAllTextAsync(dataFilePath);
    var items = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? [];
    var createdItems = 0;
    var skippedItems = 0;

    foreach (var item in items)
    {
        var createItem = false;
        var id = ((string)item.id).ToLowerInvariant();
        var brewerId = ((string)item.BrewerId).ToLowerInvariant();
        var entityType = ((string)item.EntityType);
        var partitionKey = new PartitionKeyBuilder()
            .Add(brewerId)
            .Add(entityType)
            .Build();

        try
        {
            await container.ReadItemAsync<dynamic>(id, partitionKey);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            createItem = true;
        }

        if (!createItem)
        {
            skippedItems++;
            continue;
        }

        try
        {
            var createResponse = await container.CreateItemAsync(item, partitionKey);
            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                Console.WriteLine($"Failed to create beer entity with id: {id}");
            }
            else
            {
                createdItems++;
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            skippedItems++;
        }
    }

    return (items.Count, createdItems, skippedItems);
}

async Task<bool> EnsureBrewerReviewDataFile(string brewersPath, string reviewsPath)
{
    if (!File.Exists(brewersPath))
    {
        Console.WriteLine($"Unable to locate brewers data file: {brewersPath}");
        return false;
    }

    var brewersJson = await File.ReadAllTextAsync(brewersPath);
    var brewers = JsonConvert.DeserializeObject<List<dynamic>>(brewersJson) ?? [];
    if (brewers.Count == 0)
    {
        Console.WriteLine("No brewer records found in the seed source.");
        return false;
    }

    var reviews = new List<object>();
    var now = DateTime.UtcNow;
    var reviewerNames = new[]
    {
        "Sam Taproom", "Casey Malt", "Jordan Hop", "Taylor Flight",
        "Morgan Keg", "Riley Foam", "Alex Pint", "Jamie Barrel"
    };
    var reviewTitles = new[]
    {
        "Outstanding lineup", "Great seasonal release", "Solid core beers", "Impressive consistency",
        "Fantastic tasting room", "Worth the hype", "Great value for quality", "Excellent craft portfolio"
    };
    var reviewComments = new[]
    {
        "The beer list is deep, balanced, and very well executed.",
        "Seasonals were fresh and distinct. Would recommend to friends.",
        "Core selections are consistent and easy to enjoy any time.",
        "A strong brewery identity with reliable quality across styles.",
        "Taproom service and beer quality were both top tier.",
        "Strong aroma, clean finish, and memorable flavor profile.",
        "Great quality for price; several options were standouts.",
        "Creative, polished releases with good style variety."
    };

    foreach (var brewer in brewers)
    {
        var brewerId = ((string)brewer.BrewerId)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(brewerId))
        {
            continue;
        }

        for (var i = 0; i < 12; i++)
        {
            var rating = (i % 5) + 1;
            var created = now.AddDays(-(i * 3));
            var deterministicSeed = $"brewerreview|v1|{brewerId}|{i:D2}";
            reviews.Add(new
            {
                id = CreateDeterministicGuid(deterministicSeed),
                EntityType = BeersDataLoader.PartitionKeyConstants.BrewerReview,
                BrewerId = brewerId,
                ReviewerName = reviewerNames[i % reviewerNames.Length],
                Title = reviewTitles[i % reviewTitles.Length],
                Comments = reviewComments[i % reviewComments.Length],
                Rating = rating,
                isDeletable = true,
                CreatedBy = "stanley.hudson.AdventureWorks@mickletofsky.com",
                ModifiedBy = "jim.halpert.AdventureWorks@mickletofsky.com",
                CreatedDate = created.ToString("O"),
                ModifiedDate = created.ToString("O")
            });
        }
    }

    await File.WriteAllTextAsync(reviewsPath, JsonConvert.SerializeObject(reviews, Formatting.Indented));
    return true;
}

string CreateDeterministicGuid(string seed)
{
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
    var bytes = new byte[16];
    Array.Copy(hash, bytes, 16);
    return new Guid(bytes).ToString().ToLowerInvariant();
}
