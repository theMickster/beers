namespace Beers.Common.Settings;

public sealed class CosmosDbConnectionSettings
{
    public string Account { get; set; } = string.Empty;

    public string SecurityKey { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

}
