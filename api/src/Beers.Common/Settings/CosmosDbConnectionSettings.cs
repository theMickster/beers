namespace Beers.Common.Settings;

public sealed class CosmosDbConnectionSettings
{
    public const string SettingsRootName = "AzureCosmosDb";

    public string? Account { get; set; }

    public string? SecurityKey { get; set; }

    public string? DatabaseName { get; set; }

}
