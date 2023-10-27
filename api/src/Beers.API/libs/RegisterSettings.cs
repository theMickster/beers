using Beers.Application.Exceptions;
using Beers.Common.Settings;

namespace Beers.API.libs;

internal static class RegisterSettings
{
    internal static WebApplicationBuilder RegisterCommonSettings(this WebApplicationBuilder builder)
    {
        // ******* Access the configuration manager *******
        var configuration = builder.Configuration;

        var cosmosDbSettings = configuration.GetSection(CosmosDbConnectionSettings.SettingsRootName) ?? 
                               throw new ConfigurationException("The required Configuration settings keys for the Azure Cosmos Db Settings are missing. Please verify configuration.");
        builder.Services.AddOptions<CosmosDbConnectionSettings>().Bind(cosmosDbSettings);
        
        return builder;
    }
}
