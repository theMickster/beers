namespace Beers.Common.Constants;

public static class CosmosContainerConstants
{
    /// <summary>
    /// CosmosDb container used for storing beer data.
    /// </summary>
    public const string MainContainer = "beers";

    /// <summary>
    /// CosmosDb container used for storing supporting beer metadata.
    /// </summary>
    public const string MetadataContainer = "beersMetadata";
}
