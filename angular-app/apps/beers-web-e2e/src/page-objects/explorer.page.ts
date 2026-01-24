import type { Locator, Page, Route } from '@playwright/test';

export const STYLE_IPA = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
export const STYLE_STOUT = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';
export const CATEGORY_DOMESTIC = '3f2504e0-4f89-11d3-9a0c-0305e82c3303';
export const TYPE_ALE = '3f2504e0-4f89-11d3-9a0c-0305e82c3304';

interface MockBeer {
  beerId: string;
  name: string;
  styleId: string;
  price: number;
}

/** 15 beers: odd ones are IPAs priced 5-9, even ones are stouts priced 10-14. */
export const BEERS: readonly MockBeer[] = Array.from({ length: 15 }, (_, i) => {
  const n = i + 1;
  const isIpa = n % 2 === 1;
  return {
    beerId: `beer-${String(n).padStart(2, '0')}`,
    name: `Beer ${String(n).padStart(2, '0')}`,
    styleId: isIpa ? STYLE_IPA : STYLE_STOUT,
    price: isIpa ? 5 + (n % 5) : 10 + (n % 5),
  };
});

export interface SearchCall {
  readonly pageNumber: string | null;
  readonly pageSize: string | null;
  readonly body: Record<string, unknown>;
}

function toApiBeer(beer: MockBeer) {
  return {
    beerId: beer.beerId,
    brewerId: 'brewer-1',
    name: beer.name,
    description: '',
    image: '',
    sku: beer.beerId,
    isDeletable: false,
    rating: { average: 4.5, reviewCount: 3 },
    pricing: [
      { price: beer.price, quantity: 6, unitVolume: 12, packaging: 'Can' },
    ],
    brewer: {
      id: 'brewer-1',
      name: 'Mock Brewing',
      website: 'https://mock.test',
    },
    beerType: { id: TYPE_ALE, name: 'Ale' },
    beerCategories: [],
    beerStyles: [{ id: beer.styleId, name: 'Style' }],
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-01T00:00:00Z',
  };
}

export class ExplorerPage {
  readonly heading: Locator;
  readonly cards: Locator;
  readonly loading: Locator;
  readonly emptyState: Locator;
  readonly errorAlert: Locator;
  readonly resultCount: Locator;
  readonly minPrice: Locator;
  readonly maxPrice: Locator;
  readonly applyPrice: Locator;
  readonly priceError: Locator;
  readonly pageSize: Locator;
  readonly pagination: Locator;

  /** Every `POST /beers/search` the page made, in order. */
  readonly searches: SearchCall[] = [];

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { level: 1, name: 'Explorer' });
    this.cards = page.locator('bw-beer-card');
    this.loading = page.getByTestId('loading');
    this.emptyState = page.locator('bw-empty-state');
    this.errorAlert = page.locator('.alert-error');
    this.resultCount = page.getByTestId('result-count');
    this.minPrice = page.getByLabel('Minimum price');
    this.maxPrice = page.getByLabel('Maximum price');
    this.applyPrice = page.getByRole('button', { name: 'Apply price' });
    this.priceError = page.locator('#explorer-price-error');
    this.pageSize = page.getByLabel('Per page');
    this.pagination = page.getByRole('navigation', { name: 'Pagination' });
  }

  /** Mocks the metadata lists and a server-like `/beers/search`. Call before `goto`. */
  async mockApi(options: { searchStatus?: number } = {}): Promise<void> {
    await this.page.route('**/api/v1/beerStyles', (route) =>
      route.fulfill({
        json: [
          { id: STYLE_IPA, name: 'IPA' },
          { id: STYLE_STOUT, name: 'Stout' },
        ],
      }),
    );
    await this.page.route('**/api/v1/beerCategories', (route) =>
      route.fulfill({ json: [{ id: CATEGORY_DOMESTIC, name: 'Domestic' }] }),
    );
    await this.page.route('**/api/v1/beerTypes', (route) =>
      route.fulfill({ json: [{ id: TYPE_ALE, name: 'Ale' }] }),
    );
    await this.page.route('**/api/v1/breweryTypes', (route) =>
      route.fulfill({ json: [] }),
    );
    await this.page.route('**/api/v1/beers/search**', (route) =>
      this.fulfillSearch(route, options.searchStatus),
    );
  }

  private async fulfillSearch(route: Route, status?: number): Promise<void> {
    const url = new URL(route.request().url());
    const body = (route.request().postDataJSON() ?? {}) as Record<
      string,
      unknown
    >;
    const pageNumber = url.searchParams.get('PageNumber');
    const pageSize = url.searchParams.get('PageSize');
    this.searches.push({ pageNumber, pageSize, body });

    if (status !== undefined) {
      await route.fulfill({ status, json: ['Mock failure.'] });
      return;
    }

    const styles = (body['beerStyleIds'] as string[] | undefined) ?? [];
    const min = body['minPrice'] as number | undefined;
    const max = body['maxPrice'] as number | undefined;
    const matches = BEERS.filter(
      (beer) =>
        (styles.length === 0 || styles.includes(beer.styleId)) &&
        (min === undefined || beer.price >= min) &&
        (max === undefined || beer.price <= max),
    );

    const size = Number(pageSize ?? 10);
    const number = Number(pageNumber ?? 1);
    const totalPages = Math.ceil(matches.length / size);
    await route.fulfill({
      json: {
        pageNumber: number,
        pageSize: size,
        totalPages,
        hasPreviousPage: number > 1,
        hasNextPage: number < totalPages,
        totalRecords: matches.length,
        results: matches
          .slice((number - 1) * size, number * size)
          .map(toApiBeer),
      },
    });
  }

  async goto(query = ''): Promise<void> {
    await this.page.goto(`/explorer${query}`);
  }

  checkbox(label: string): Locator {
    return this.page.getByRole('checkbox', { name: label, exact: true });
  }

  pageButton(name: string): Locator {
    return this.pagination.getByRole('button', { name, exact: true });
  }

  card(name: string): Locator {
    return this.cards.filter({ hasText: name });
  }
}
