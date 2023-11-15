using Beers.Common.Constants;
using Beers.Domain.Entities.Base;

namespace Beers.Domain.Entities;

public class BrewerEntity : BaseBeerEntity
{
    public string EntityType = PartitionKeyConstants.Brewer;

    public string Name { get; set; }

    public int FoundedIn { get; set; }

    public string Headquarters { get; set; }

    public string Website { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
