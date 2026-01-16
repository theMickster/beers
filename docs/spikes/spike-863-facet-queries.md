# Spike #863: Facet Count Queries

Queries to retire the #863 spike risks. IDs come from `tools/dataLoader/Data/Beers.json`, and expected results are calculated from that same file. Data Explorer doesn't support `@parameters`, so the values are inline literals. The service will use `QueryDefinition.WithParameter`.

Run each query separately in Data Explorer → `Beers` container → New SQL Query.

## Q0: Baseline (GROUP BY over JOIN, no subquery)

```sql
SELECT s.metadataId AS Id, s.name AS Name, COUNT(1) AS BeerCount
FROM c
JOIN s IN c.BeerStyles
WHERE c.EntityType = 'Beer'
GROUP BY s.metadataId, s.name
```

Expected: 23 rows. American IPA = 17, Double/Imperial IPA = 11, Hazy IPA = 8. The counts add up to 79, more than the 70 beers, because 9 beers have two styles.

## Q1: The risk (style counts with an `EXISTS` category filter for Domestic OR Premium, MinRating 4.0)

```sql
SELECT s.metadataId AS Id, s.name AS Name, COUNT(1) AS BeerCount
FROM c
JOIN s IN c.BeerStyles
WHERE c.EntityType = 'Beer'
  AND c.Rating.average >= 4.0
  AND EXISTS (
      SELECT VALUE cat FROM cat IN c.BeerCategories
      WHERE cat.metadataId IN ('750bddce-3abc-4585-9000-e045c8aba866', '8381fbee-5972-40dc-adb9-642bf2e1f337'))
GROUP BY s.metadataId, s.name
```

Expected: 7 rows from 7 matching beers. American IPA = 2, and Amber, American Style Light Lager, American-Style Lager, Czech Pilsner, Red, Specialty Beer = 1 each.

## Q2: Fallback (same filter with `ARRAY_CONTAINS`, no subquery)

```sql
SELECT s.metadataId AS Id, s.name AS Name, COUNT(1) AS BeerCount
FROM c
JOIN s IN c.BeerStyles
WHERE c.EntityType = 'Beer'
  AND c.Rating.average >= 4.0
  AND (ARRAY_CONTAINS(c.BeerCategories, {"metadataId": "750bddce-3abc-4585-9000-e045c8aba866"}, true)
    OR ARRAY_CONTAINS(c.BeerCategories, {"metadataId": "8381fbee-5972-40dc-adb9-642bf2e1f337"}, true))
GROUP BY s.metadataId, s.name
```

Expected: identical to Q1.

## Q3: Brewer counts with an `EXISTS` style filter for American IPA OR Hazy IPA (no JOIN)

```sql
SELECT c.Brewer.id AS Id, c.Brewer.name AS Name, COUNT(1) AS BeerCount
FROM c
WHERE c.EntityType = 'Beer'
  AND c.Rating.average >= 4.0
  AND EXISTS (
      SELECT VALUE s FROM s IN c.BeerStyles
      WHERE s.metadataId IN ('46c48c77-b143-4dc9-ad57-a797e0dc8997', '1e65b3a8-6049-4d96-9f54-46a24e367182'))
GROUP BY c.Brewer.id, c.Brewer.name
```

Expected: 7 rows from 16 beers. Bootstrap, Founders, Sierra Nevada = 3 each. Dogfish Head, Odell, Oskar Blues = 2 each. New Belgium = 1. Lagunitas and Molson Coors don't appear at all, which also checks the "Zero Counts" scenario.

## Q4: Multi-brewer `IN` routing

```sql
SELECT VALUE COUNT(1) FROM c
WHERE c.EntityType = 'Beer' AND c.BrewerId = '02c751e8-c565-4698-8f57-2cde3e2f8f61'
```

```sql
SELECT VALUE COUNT(1) FROM c
WHERE c.EntityType = 'Beer'
  AND c.BrewerId IN ('02c751e8-c565-4698-8f57-2cde3e2f8f61', 'fa25893a-5993-491b-bd22-a9ce1e61dbd0')
```

Expected: 12 for Odell alone, 21 for Odell plus Founders. Compare the RU charge for each on the Query Stats tab.

This probably can't tell you much. With 70 beers there's almost certainly one physical partition, so routed and fan-out queries hit the same place and look identical. If they do, close the risk as "can't be observed at this data size". Single-brewer equality routing is documented either way.

## Reading the Results

| Outcome | Decision for #863 |
| --- | --- |
| Q1 and Q3 match the expected results | Use `EXISTS` for every array-facet filter |
| Q1 or Q3 errors, Q2 matches | Use the `ARRAY_CONTAINS` fallback. The 25-ID cap in #865 keeps the OR chain short |
| Q0 errors | GROUP BY over a JOIN itself doesn't work here. Count facets in memory from the filtered result instead (cheap at 70 beers) |
| Queries run but the counts differ | The container has drifted from `Beers.json`, for example beers added or deleted through the API. Re-check with a plain non-grouped query before drawing conclusions |

## Results

Run in Data Explorer against the `Beers` container. Every query matched the expected rows and counts.

| Query | Result | RU |
| --- | --- | --- |
| Q0 | 23 rows, counts sum to 79. GROUP BY over a JOIN works | 6.69 |
| Q1 | 7 rows. `EXISTS` category filter with JOIN and GROUP BY works | 3.99 |
| Q2 | Same 7 rows as Q1. `ARRAY_CONTAINS` works as a fallback | 3.96 |
| Q3 | 7 rows from 16 beers. `EXISTS` style filter with no JOIN works | 4.18 |
| Q4 | 12 for Odell alone, 21 for Odell plus Founders | 3.09 and 3.19 |

**Decision for #863:** use `EXISTS` for every array-facet filter. `EXISTS` and `ARRAY_CONTAINS` cost about the same, so the fallback isn't needed.

- **Zero counts:** Lagunitas and Molson Coors are absent from Q3, so GROUP BY returns no row for a facet value with no matching beers. The service has to add zero-count rows itself for any facet value it wants to show.
- **Q4 routing:** 3.09 vs 3.19 RU is within noise. At 70 beers the routed and multi-brewer queries can't be told apart, so the risk is closed as "can't be observed at this data size".
- **Scale:** every query costs single-digit RUs at this size, which says nothing about cost at scale. Re-measure on a larger dataset if volume grows.

## Caveats

- The alias is `BeerCount` instead of the `Count` in the #863 notes, because it's unconfirmed whether `Count` is safe as an alias in Cosmos SQL. The model property can still be named `Count`.
- Data Explorer passing is strong evidence but not complete proof. The .NET SDK builds its own query pipeline for cross-partition GROUP BY. The final confirmation is the #863 service running these queries through `CosmosClient`.
