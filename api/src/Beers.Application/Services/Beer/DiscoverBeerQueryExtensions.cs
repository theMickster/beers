using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Entities;

namespace Beers.Application.Services.Beer;

/// <summary>
/// EF Core filter and sort rules for the discover endpoint. Values within one filter are OR'd; different filters are AND'd.
/// </summary>
internal static class DiscoverBeerQueryExtensions
{
    public static IQueryable<BeerEntity> ApplyFilters(this IQueryable<BeerEntity> query, FacetedSearchInputModel input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.BeerStyleIds.Count > 0)
        {
            var ids = input.BeerStyleIds;
            query = query.Where(x => x.BeerStyles.Any(s => ids.Contains(s.MetadataId)));
        }

        if (input.BeerCategoryIds.Count > 0)
        {
            var ids = input.BeerCategoryIds;
            query = query.Where(x => x.BeerCategories.Any(c => ids.Contains(c.MetadataId)));
        }

        if (input.BrewerIds.Count == 1)
        {
            var brewerId = input.BrewerIds[0];
            query = query.Where(x => x.BrewerId == brewerId);
        }
        else if (input.BrewerIds.Count > 1)
        {
            var ids = input.BrewerIds;
            query = query.Where(x => ids.Contains(x.BrewerId));
        }

        if (input.BeerTypeIds.Count > 0)
        {
            var ids = input.BeerTypeIds;
            query = query.Where(x => ids.Contains(x.BeerType.MetadataId));
        }

        if (input.MinRating.HasValue)
        {
            var min = input.MinRating.Value;
            query = query.Where(x => x.Rating.Average >= min);
        }

        if (input.MaxRating.HasValue)
        {
            var max = input.MaxRating.Value;
            query = query.Where(x => x.Rating.Average <= max);
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            var name = input.Name.Trim();
            query = query.Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }

    public static IQueryable<BeerEntity> ApplySort(this IQueryable<BeerEntity> query, DiscoverBeerParameter parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var descending = parameters.SortOrder == SortedResultConstants.Descending;

        if (parameters.OrderBy == SortedResultConstants.ReviewCount)
        {
            return descending
                ? query.OrderByDescending(x => x.Rating.ReviewCount).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.Rating.ReviewCount).ThenBy(x => x.Name);
        }

        if (parameters.OrderBy == SortedResultConstants.CreatedDate)
        {
            return descending
                ? query.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.CreatedDate).ThenBy(x => x.Name);
        }

        return descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
    }
}
