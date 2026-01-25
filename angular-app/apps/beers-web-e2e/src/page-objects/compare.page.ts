import type { Locator, Page } from '@playwright/test';
import { BEER } from './beer-detail.page';

export const guid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c33${String(n).padStart(2, '0')}`;

export interface CompareMockBeer {
  readonly name: string;
  readonly prices: readonly number[];
  readonly rating: { average: number; reviewCount: number } | null;
  readonly styles: readonly string[];
}

const DETAIL_URL = /\/api\/v1\/beers\/[^/?]+$/;

export class ComparePage {
  readonly heading: Locator;
  readonly loading: Locator;
  readonly prompt: Locator;
  readonly capped: Locator;
  readonly table: Locator;
  readonly names: Locator;
  readonly prices: Locator;
  readonly ratings: Locator;
  readonly styles: Locator;

  /** Every `GET /beers/{id}` the browser made, in order. */
  readonly detailRequests: string[] = [];

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { level: 1, name: 'Compare' });
    this.loading = page.getByTestId('loading');
    this.prompt = page.getByTestId('compare-empty');
    this.capped = page.getByTestId('compare-capped');
    this.table = page.getByTestId('compare-table');
    this.names = page.getByTestId('compare-name');
    this.prices = page.getByTestId('compare-price');
    this.ratings = page.getByTestId('compare-rating');
    this.styles = page.getByTestId('compare-styles');

    page.on('request', (request) => {
      if (request.method() === 'GET' && DETAIL_URL.test(request.url())) {
        this.detailRequests.push(request.url());
      }
    });
  }

  /** Answers `GET /beers/{id}` with a beer, or with `status` and a message body. Call before `goto`. */
  async mockBeer(id: string, beer: CompareMockBeer | number): Promise<void> {
    await this.page.route(`**/api/v1/beers/${id}`, (route) =>
      typeof beer === 'number'
        ? route.fulfill({ status: beer, json: ['Mock failure.'] })
        : route.fulfill({
            json: {
              ...BEER,
              beerId: id,
              name: beer.name,
              rating: beer.rating,
              pricing: beer.prices.map((price) => ({
                price,
                quantity: 6,
                unitVolume: 12,
                packaging: 'Can',
              })),
              beerStyles: beer.styles.map((name, i) => ({
                id: guid(90 + i),
                name,
              })),
            },
          }),
    );
  }

  /**
   * The server already renders the prompt, so its visibility proves nothing about the client.
   * Angular strips `ngh` from the root as it hydrates; a frame later the first render, and any
   * request an effect starts from it, has happened.
   */
  async waitForHydration(): Promise<void> {
    await this.page
      .locator('bw-root:not([ngh])')
      .waitFor({ state: 'attached' });
    await this.page.evaluate(
      () =>
        new Promise<void>((resolve) => requestAnimationFrame(() => resolve())),
    );
  }

  async goto(...ids: string[]): Promise<void> {
    await this.page.goto(
      ids.length === 0 ? '/compare' : `/compare?beers=${ids.join(',')}`,
    );
  }
}
