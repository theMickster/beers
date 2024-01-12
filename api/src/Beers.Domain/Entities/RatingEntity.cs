using Newtonsoft.Json;

namespace Beers.Domain.Entities;
public sealed class RatingEntity
{
    [JsonProperty("average")]
    public decimal Average { get; set; }

    [JsonProperty("reviews")]
    public int ReviewCount { get; set; }
}
