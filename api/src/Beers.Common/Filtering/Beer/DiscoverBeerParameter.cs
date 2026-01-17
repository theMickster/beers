using Beers.Common.Constants;
using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.Beer;

/// <summary>
/// Used to support paging and sorting while discovering Beer records.
/// </summary>
public sealed class DiscoverBeerParameter : SearchParameterBase
{
    private const string NameField = "Name";
    private const string ReviewCountField = "ReviewCount";
    private const string CreatedDateField = "CreatedDate";
    private string _orderBy = NameField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                ReviewCountField => SortedResultConstants.ReviewCount,
                CreatedDateField => SortedResultConstants.CreatedDate,
                _ => SortedResultConstants.Name
            };
        }
        set =>
            _orderBy = value.Trim().Equals(ReviewCountField, StringComparison.CurrentCultureIgnoreCase) ? ReviewCountField
                : value.Trim().Equals(CreatedDateField, StringComparison.CurrentCultureIgnoreCase) ? CreatedDateField : NameField;
    }
}
