import type { Locator, Page } from '@playwright/test';

export const guid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c33${String(n).padStart(2, '0')}`;

export const TYPE_MICRO = { id: guid(90), name: 'Micro' };
export const TYPE_REGIONAL = { id: guid(91), name: 'Regional' };

export interface MockBrewer {
  readonly brewerId: string;
  readonly name: string;
  readonly foundedIn: number;
  readonly headquarters: string;
  readonly website: string;
  readonly breweryType: { id: string; name: string };
  readonly rating: { average: number; reviewCount: number } | null;
}

export function toApiBrewer(brewer: MockBrewer) {
  return {
    ...brewer,
    isDeletable: false,
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-01T00:00:00Z',
  };
}

/** 10 brewers: "Brewer 01" to "Brewer 06" are Micro, "Brewer 07" to "Brewer 10" are Regional. */
export const BREWERS: readonly MockBrewer[] = Array.from(
  { length: 10 },
  (_, i) => {
    const n = i + 1;
    return {
      brewerId: guid(n),
      name: `Brewer ${String(n).padStart(2, '0')}`,
      foundedIn: 1980 + n,
      headquarters: `Town ${n}, CO`,
      website: `https://brewer-${n}.test`,
      breweryType: n <= 6 ? TYPE_MICRO : TYPE_REGIONAL,
      rating: n === 1 ? { average: 4.5, reviewCount: 3 } : null,
    };
  },
);

export class BrewerDirectoryPage {
  readonly heading: Locator;
  readonly cards: Locator;
  readonly loading: Locator;
  readonly typeSelect: Locator;
  readonly pagination: Locator;
  readonly errorAlert: Locator;

  /** Every `GET /brewers` the browser made. */
  readonly listRequests: string[] = [];

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { level: 1, name: 'Brewers' });
    this.cards = page.getByTestId('brewer-card');
    this.loading = page.getByTestId('loading');
    this.typeSelect = page.getByLabel('Brewery type');
    this.pagination = page.getByRole('navigation', { name: 'Pagination' });
    this.errorAlert = page.locator('.alert-error');
  }

  /** Answers `GET /brewers` with the list, or with `status`. Call before `goto`. */
  async mockApi(
    brewers: readonly MockBrewer[] = BREWERS,
    status?: number,
  ): Promise<void> {
    await this.page.route('**/api/v1/brewers', (route) => {
      this.listRequests.push(route.request().url());
      return status === undefined
        ? route.fulfill({ json: brewers.map(toApiBrewer) })
        : route.fulfill({ status, json: ['Mock failure.'] });
    });
  }

  async goto(query = ''): Promise<void> {
    await this.page.goto(`/brewers${query}`);
  }

  card(name: string): Locator {
    return this.cards.filter({ hasText: name });
  }

  pageButton(name: string): Locator {
    return this.pagination.getByRole('button', { name, exact: true });
  }
}
