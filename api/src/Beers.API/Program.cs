using Beers.API.libs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", false, true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddInMemoryCollection()
    .Build();

builder.Configuration.RegisterApplicationConfiguration();

builder.Services.AddLogging(builder.Configuration);

builder.Services.AddOptions();

builder.Services.AddMemoryCache();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.RegisterCommonSettings();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BeersApiCorsPolicy",
        b => b
            .SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});


builder.RegisterAspDotNetServices();

builder.RegisterServicesViaReflection();

builder.RegisterDataServices();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

await builder.Build().SetupMiddleware().RunAsync();

/// <summary>
/// The entry point for the API.
/// </summary>
/// <remarks>
/// Declared this way to bypass the unit test code coverage analysis
/// </remarks>
[ExcludeFromCodeCoverage]
public partial class Program { }

