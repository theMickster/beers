namespace Beers.Common.Settings;

public sealed class CosmosDbConnectionSettings
{
    public const string SettingsRootName = "AzureCosmosDb";

    public string Account { get; set; } = string.Empty;

    public string SecurityKey { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

}
