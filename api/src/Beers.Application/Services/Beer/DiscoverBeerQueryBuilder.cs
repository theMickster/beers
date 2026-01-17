using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Microsoft.Azure.Cosmos;

namespace Beers.Application.Services.Beer;

public enum DiscoverFacet
{
    Style,
    Category,
    Brewer,
    BeerType
}

/// <summary>
/// A single GROUP BY row returned by a facet query.
/// </summary>
public sealed class FacetRow
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int BeerCount { get; set; }
}

/// <summary>
/// Builds the Cosmos SQL for one facet count query. Each query applies every filter except the one for the facet being counted,
/// so a facet always shows the options still available after the user's other selections.
/// </summary>
internal static class DiscoverBeerQueryBuilder
{
    public static QueryDefinition Build(FacetedSearchInputModel input, DiscoverFacet facet)
    {
        ArgumentNullException.ThrowIfNull(input);

        var conditions = new List<string> { "c.EntityType = @entityType" };
        var parameters = new List<(string Name, object Value)> { ("@entityType", PartitionKeyConstants.Beer) };

        if (facet != DiscoverFacet.Style)
        {
            AddArrayFilter(conditions, parameters, "BeerStyles", "style", input.BeerStyleIds);
        }

        if (facet != DiscoverFacet.Category)
        {
            AddArrayFilter(conditions, parameters, "BeerCategories", "category", input.BeerCategoryIds);
        }

        if (facet != DiscoverFacet.Brewer)
        {
            AddBrewerFilter(conditions, parameters, input.BrewerIds);
        }

        if (facet != DiscoverFacet.BeerType)
        {
            AddTypeFilter(conditions, parameters, input.BeerTypeIds);
        }

        if (input.MinRating.HasValue)
        {
            conditions.Add("c.Rating.average >= @minRating");
            parameters.Add(("@minRating", input.MinRating.Value));
        }

        if (input.MaxRating.HasValue)
        {
            conditions.Add("c.Rating.average <= @maxRating");
            parameters.Add(("@maxRating", input.MaxRating.Value));
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            conditions.Add("CONTAINS(c.Name, @name, true)");
            parameters.Add(("@name", input.Name.Trim()));
        }

        var sql = $"{SelectAndFrom(facet)} WHERE {string.Join(" AND ", conditions)} {GroupBy(facet)}";
        var query = new QueryDefinition(sql);
        foreach (var (name, value) in parameters)
        {
            query.WithParameter(name, value);
        }

        return query;
    }

    private static string SelectAndFrom(DiscoverFacet facet) => facet switch
    {
        DiscoverFacet.Style =>
            "SELECT s.metadataId AS Id, s.name AS Name, COUNT(1) AS BeerCount FROM c JOIN s IN c.BeerStyles",
        DiscoverFacet.Category =>
            "SELECT cat.metadataId AS Id, cat.name AS Name, COUNT(1) AS BeerCount FROM c JOIN cat IN c.BeerCategories",
        DiscoverFacet.Brewer =>
            "SELECT c.Brewer.id AS Id, c.Brewer.name AS Name, COUNT(1) AS BeerCount FROM c",
        DiscoverFacet.BeerType =>
            "SELECT c.BeerType.metadataId AS Id, c.BeerType.name AS Name, COUNT(1) AS BeerCount FROM c",
        _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null)
    };

    private static string GroupBy(DiscoverFacet facet) => facet switch
    {
        DiscoverFacet.Style => "GROUP BY s.metadataId, s.name",
        DiscoverFacet.Category => "GROUP BY cat.metadataId, cat.name",
        DiscoverFacet.Brewer => "GROUP BY c.Brewer.id, c.Brewer.name",
        DiscoverFacet.BeerType => "GROUP BY c.BeerType.metadataId, c.BeerType.name",
        _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null)
    };

    private static void AddArrayFilter(List<string> conditions, List<(string Name, object Value)> parameters,
        string arrayProperty, string prefix, List<Guid> ids)
    {
        var names = AddIdParameters(parameters, prefix, ids);
        if (names.Count == 0)
        {
            return;
        }

        conditions.Add(
            $"EXISTS (SELECT VALUE x FROM x IN c.{arrayProperty} WHERE x.metadataId IN ({string.Join(", ", names)}))");
    }

    private static void AddBrewerFilter(List<string> conditions, List<(string Name, object Value)> parameters, List<Guid> ids)
    {
        var names = AddIdParameters(parameters, "brewer", ids);
        if (names.Count == 0)
        {
            return;
        }

        // A single equality on BrewerId targets the hierarchical partition key prefix.
        conditions.Add(names.Count == 1
            ? $"c.BrewerId = {names[0]}"
            : $"c.BrewerId IN ({string.Join(", ", names)})");
    }

    private static void AddTypeFilter(List<string> conditions, List<(string Name, object Value)> parameters, List<Guid> ids)
    {
        var names = AddIdParameters(parameters, "type", ids);
        if (names.Count == 0)
        {
            return;
        }

        conditions.Add($"c.BeerType.metadataId IN ({string.Join(", ", names)})");
    }

    private static List<string> AddIdParameters(List<(string Name, object Value)> parameters, string prefix, List<Guid> ids)
    {
        var names = new List<string>();
        foreach (var id in ids.Distinct())
        {
            var name = $"@{prefix}{names.Count}";
            names.Add(name);
            parameters.Add((name, id.ToString("D").ToLowerInvariant()));
        }

        return names;
    }
}
