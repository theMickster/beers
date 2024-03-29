﻿using Beers.API.Helpers;
using Beers.Application.Exceptions;
using Beers.Common.Helpers;

namespace Beers.API.libs;

[ExcludeFromCodeCoverage]
internal static class RegisterAppConfiguration
{
    internal static void RegisterApplicationConfiguration(this IConfiguration configuration)
    {
        var akvClient = AzureKeyVaultDataHelper.GetAzureKeyVaultClient(configuration);

        if (akvClient == null)
        {
            throw new ConfigurationException(
                "Unable to create an Azure Key Vault secret helper. " +
                "A properly configured helper is required. " +
                "Please verify local or Azure resource configuration. ");
        }

        SecretHelper.SecretClient = akvClient;
    }
}
