using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace BeersDataLoader.Configuration;

internal static class LoaderConfigurationBuilder
{
    internal static IConfiguration Build()
    {
        return new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
