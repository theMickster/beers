namespace Beers.Common.Settings;
public sealed class CacheSettings
{
    public const string SettingsRootName = "CacheSettings";

    public int TimeoutInSeconds { get; set; }

}
