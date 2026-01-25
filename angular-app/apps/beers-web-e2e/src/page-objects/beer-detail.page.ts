import type { Locator, Page } from '@playwright/test';

export const BEER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
export const BREWER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';
export const MISSING_BEER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3399';

export const BEER = {
  beerId: BEER_ID,
  brewerId: BREWER_ID,
  name: 'Hazy Day',
  description: 'A juicy, unfiltered IPA.',
  image: '',
  sku: 'hazy-day',
  isDeletable: false,
  rating: { average: 4.25, reviewCount: 12 },
  pricing: [
    { price: 9.99, quantity: 6, unitVolume: 12, packaging: 'Can' },
    { price: 18, quantity: 12, unitVolume: 16, packaging: 'Bottle' },
  ],
  brewer: { id: BREWER_ID, name: 'Odell', website: 'https://odell.test' },
  beerType: { id: '3f2504e0-4f89-11d3-9a0c-0305e82c3304', name: 'Ale' },
  beerCategories: [],
  beerStyles: [],
  createdDate: '2026-01-01T00:00:00Z',
  modifiedDate: '2026-01-01T00:00:00Z',
};

const DETAIL_URL = /\/api\/v1\/beers\/[^/?]+$/;

export class BeerDetailPage {
  readonly heading: Locator;
  readonly loading: Locator;
  readonly notFound: Locator;
  readonly errorAlert: Locator;
  readonly rating: Locator;
  readonly brewerLink: Locator;
  readonly pricingRows: Locator;
  readonly compareToggle: Locator;

  /** Every `GET /beers/{id}` the browser made. */
  readonly detailRequests: string[] = [];

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { level: 1 });
    this.loading = page.getByTestId('loading');
    this.notFound = page.getByTestId('not-found');
    this.errorAlert = page.locator('.alert-error');
    this.rating = page.getByTestId('beer-rating');
    this.brewerLink = page.getByTestId('brewer-link');
    this.pricingRows = page.locator('[data-testid="pricing-table"] tbody tr');
    this.compareToggle = page.getByTestId('compare-toggle');

    page.on('request', (request) => {
      if (request.method() === 'GET' && DETAIL_URL.test(request.url())) {
        this.detailRequests.push(request.url());
      }
    });
  }

  /** Answers `GET /beers/{id}` with the beer, or with `status` and a message body. Call before `goto`. */
  async mockBeer(id: string, status?: number): Promise<void> {
    await this.page.route(`**/api/v1/beers/${id}`, (route) =>
      status === undefined
        ? route.fulfill({ json: { ...BEER, beerId: id } })
        : route.fulfill({ status, json: ['Mock failure.'] }),
    );
  }

  async goto(id: string): Promise<void> {
    await this.page.goto(`/beers/${id}`);
  }
}
