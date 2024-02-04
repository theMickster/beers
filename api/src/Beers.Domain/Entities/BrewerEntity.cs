using Beers.Common.Constants;
using Beers.Domain.Entities.Base;
using Beers.Domain.Entities.Slims;

namespace Beers.Domain.Entities;

public class BrewerEntity : BaseBeerEntity
{
    public string EntityType = PartitionKeyConstants.Brewer;

    public string Name { get; set; } = string.Empty;

    public int FoundedIn { get; set; } = 2000;

    public string Headquarters { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public BreweryTypeSlimEntity BreweryType { get; set; }
}
