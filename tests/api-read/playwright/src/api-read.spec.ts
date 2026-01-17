import { expect, test } from '@playwright/test';
import { randomUUID } from 'node:crypto';

const validStatuses = [200, 404];
const randomId = () => randomUUID();

test('crawl metadata and list endpoints without server errors', async ({ request }) => {
  const paths = [
    '/swagger/v1/swagger.json',
    '/api/v1/beerTypes',
    '/api/v1/beerStyles',
    '/api/v1/beerCategories',
    '/api/v1/breweryTypes',
    '/api/v1/beers',
    '/api/v1/brewers'
  ];

  for (const path of paths) {
    const response = await request.get(path);
    expect(validStatuses).toContain(response.status());
  }
});

test('crawl read-by-id endpoints with unknown ids', async ({ request }) => {
  const beerId = randomId();
  const brewerId = randomId();
  const reviewId = randomId();

  const beerResponse = await request.get(`/api/v1/beers/${beerId}`);
  const brewerResponse = await request.get(`/api/v1/brewers/${brewerId}`);
  const reviewListResponse = await request.get(`/api/v1/brewers/${brewerId}/reviews`);
  const reviewResponse = await request.get(`/api/v1/brewers/${brewerId}/reviews/${reviewId}`);

  expect(validStatuses).toContain(beerResponse.status());
  expect(validStatuses).toContain(brewerResponse.status());
  expect(validStatuses).toContain(reviewListResponse.status());
  expect(validStatuses).toContain(reviewResponse.status());
});

test('crawl search endpoints with read-only payloads', async ({ request }) => {
  const brewerId = randomId();

  const beerSearch = await request.post('/api/v1/beers/search?PageNumber=1&PageSize=10&SortOrder=Ascending&OrderBy=beerId', {
    data: {
      id: null,
      name: '',
      brewerId: null,
      brewerName: ''
    }
  });

  expect([200, 400]).toContain(beerSearch.status());

  const reviewSearch = await request.post(`/api/v1/brewers/${brewerId}/reviews/search?PageNumber=1&PageSize=10&SortOrder=Ascending&OrderBy=reviewId`, {
    data: {
      id: null,
      name: '',
      reviewerName: '',
      minimumRating: 1,
      maximumRating: 5
    }
  });

  expect([200, 400]).toContain(reviewSearch.status());
});

type FacetItem = { id: string; name: string; count: number };

const isOrderedByCountThenName = (items: FacetItem[]) =>
  items.every((item, index) => {
    if (index === 0) {
      return true;
    }
    const previous = items[index - 1];
    return (
      previous.count > item.count ||
      (previous.count === item.count && previous.name.toLowerCase() <= item.name.toLowerCase())
    );
  });

const sumOfCounts = (items: FacetItem[]) => items.reduce((total, item) => total + item.count, 0);

test('discover beers returns ordered facets that agree with the total', async ({ request }) => {
  const response = await request.post('/api/v1/beers/discover?PageNumber=1&PageSize=10&SortOrder=Ascending&OrderBy=Name', {
    data: {}
  });

  expect(response.status()).toBe(200);

  const body = await response.json();
  expect(Array.isArray(body.results)).toBe(true);

  const facets = body.facets as Record<string, FacetItem[]>;
  for (const name of ['styleFacets', 'categoryFacets', 'brewerFacets', 'beerTypeFacets']) {
    expect(Array.isArray(facets[name])).toBe(true);
    expect(isOrderedByCountThenName(facets[name])).toBe(true);
  }

  // With no filters every beer has exactly one brewer and one beer type, so those counts sum to the total.
  expect(sumOfCounts(facets.brewerFacets)).toBe(body.totalRecords);
  expect(sumOfCounts(facets.beerTypeFacets)).toBe(body.totalRecords);
});

test('discover beers rejects an invalid rating range', async ({ request }) => {
  const response = await request.post('/api/v1/beers/discover', {
    data: { minRating: 5, maxRating: 1 }
  });

  expect(response.status()).toBe(400);
});
