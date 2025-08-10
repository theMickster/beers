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
