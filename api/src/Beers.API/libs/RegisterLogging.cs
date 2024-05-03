using Beers.Application.Exceptions;
using Beers.Common.Constants;
using Beers.Common.Helpers;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using System.Diagnostics;

namespace Beers.API.libs;

[ExcludeFromCodeCoverage]
internal static class SetupLogging
{
    internal static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        
        var appInsightsConnectionString = SecretHelper.GetSecret(AkvConstants.AzureApplicationInsightsConnectionString);
        var aspNetEnvironment = configuration.RetrieveEnvironment();
        
        if (string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.AppInsightsConnectionString} is missing." +
                "Please verify local or Azure resource configuration. Please verify that Azure Key Vault is configured properly.");
        }

        if (string.IsNullOrWhiteSpace(aspNetEnvironment))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.ApplicationEnvironment01} or " +
                $"{ConfigurationConstants.ApplicationEnvironment02} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        if (!configuration.GetSection(LogSettingsConstants.BaseLogLevelDefault).Exists())
        {
            configuration[LogSettingsConstants.BaseLogLevelDefault] =
                LogLevelConstants.BaseLogLevelDefault.ToString();
        }

        if (!configuration.GetSection(LogSettingsConstants.BaseLogLevelMicrosoft).Exists())
        {
            configuration[LogSettingsConstants.BaseLogLevelMicrosoft] =
                LogLevelConstants.BaseLogLevelMicrosoft.ToString();
        }

        if (!configuration.GetSection(LogSettingsConstants.BaseLogLevelMicrosoftHostingLifetime).Exists())
        {
            configuration[LogSettingsConstants.BaseLogLevelMicrosoftHostingLifetime] =
                LogLevelConstants.BaseLogLevelMicrosoftHostingLifetime.ToString();
        }

        _ = services.AddLogging(logBuilder =>
        {
            logBuilder.AddConfiguration(configuration.GetSection(LogSettingsConstants.Logging));

            if (configuration.GetValue(LogSettingsConstants.UseDebugLog, false))
            {
                logBuilder.AddDebug();
                Trace.TraceInformation("Add Beers API Logging:: Debug logging enabled");
            }

            if (configuration.GetValue(LogSettingsConstants.UseConsoleLog, false))
            {
                logBuilder.AddConsole();
                Trace.TraceInformation("Add Beers API Logging:: Console logging enabled");
            }

            var options = new ApplicationInsightsServiceOptions
            {
                ConnectionString = appInsightsConnectionString
            };

            _ = services.AddApplicationInsightsTelemetry(options);

        });
        return services;
    }


    #region Private Methods

    private static string RetrieveEnvironment(this IConfiguration configuration)
    {
        var environments = new[] {
            configuration[ConfigurationConstants.ApplicationEnvironment01] ?? string.Empty,
            configuration[ConfigurationConstants.ApplicationEnvironment02] ?? string.Empty
        };

        return environments.FirstOrDefault(environment => !string.IsNullOrWhiteSpace(environment)) ?? string.Empty;
    }

    private static bool ValidateRegisteredServices(IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (services.All(s => s.ServiceType != typeof(IHttpContextAccessor)))
        {
            Trace.TraceInformation(
                "IHttpContextAccessor was not added to the IServiceCollection in the correct order." +
                "The concrete implementation of IHttpContextAccessor must be registered before logging.");

            return false;
        }

        return true;
    }

    #endregion
}
