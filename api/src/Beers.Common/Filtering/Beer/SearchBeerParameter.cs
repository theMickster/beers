using Beers.Common.Constants;
using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.Beer;

/// <summary>
/// Used to support paging while searching for Beer records.
/// </summary>
public sealed class SearchBeerParameter : SearchParameterBase
{
    private const string BeerIdField = "beerId";
    private const string BeerNameField = "Name";
    private const string BrewerIdField = "brewerId";
    private string _orderBy = BeerIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                BeerIdField => SortedResultConstants.BeerId,
                BrewerIdField => SortedResultConstants.BrewerId,
                BeerNameField => SortedResultConstants.Name,
                _ => SortedResultConstants.BeerId
            };
        }
        set =>
            _orderBy = value.Trim().Equals(BeerIdField, StringComparison.CurrentCultureIgnoreCase) ? BeerIdField
                : value.Trim().Equals(BrewerIdField, StringComparison.CurrentCultureIgnoreCase) ? BrewerIdField
                : value.Trim().Equals(BeerNameField, StringComparison.CurrentCultureIgnoreCase) ? BeerNameField : BeerIdField;
    }

}
