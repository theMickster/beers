import { expect, test as base } from '@playwright/test';
import { BeerDetailPage } from './page-objects/beer-detail.page';
import {
  ComparePage,
  guid,
  type CompareMockBeer,
} from './page-objects/compare.page';
import { ExplorerPage, STYLE_IPA } from './page-objects/explorer.page';
import { ShellPage } from './page-objects/shell.page';

const test = base.extend<{
  compare: ComparePage;
  detail: BeerDetailPage;
  explorer: ExplorerPage;
  shell: ShellPage;
}>({
  compare: async ({ page }, use) => {
    await use(new ComparePage(page));
  },
  detail: async ({ page }, use) => {
    await use(new BeerDetailPage(page));
  },
  explorer: async ({ page }, use) => {
    await use(new ExplorerPage(page));
  },
  shell: async ({ page }, use) => {
    await use(new ShellPage(page));
  },
});

test.use({ viewport: { width: 1280, height: 900 } });

const [A, B, C, D, E, F] = [1, 2, 3, 4, 5, 6].map(guid);

const ALPHA: CompareMockBeer = {
  name: 'Alpha Ale',
  prices: [9.99],
  rating: { average: 4.5, reviewCount: 8 },
  styles: ['IPA', 'Hazy'],
};
const BRAVO: CompareMockBeer = {
  name: 'Bravo Brown',
  prices: [8, 14],
  rating: { average: 3.2, reviewCount: 1 },
  styles: ['Brown Ale'],
};
const CHARLIE: CompareMockBeer = {
  name: 'Charlie Stout',
  prices: [],
  rating: null,
  styles: [],
};

test.describe('compare', () => {
  test('server renders only the loading state for a link, and the prompt for a bare /compare', async ({
    page,
    compare,
  }) => {
    // `page.request` bypasses `page.route`, so only the HTML is asserted here, not API calls.
    await compare.mockBeer(A, ALPHA);
    await compare.mockBeer(B, BRAVO);

    const linked = await (
      await page.request.get(`/compare?beers=${A},${B}`)
    ).text();
    const bare = await (await page.request.get('/compare')).text();

    expect(linked).toContain('data-testid="loading"');
    expect(linked).not.toContain(ALPHA.name);
    expect(bare).toContain('data-testid="compare-empty"');
  });

  test('a pasted link renders the comparison with aligned rows and no explorer visit', async ({
    compare,
  }) => {
    await compare.mockBeer(A, ALPHA);
    await compare.mockBeer(B, BRAVO);
    await compare.mockBeer(C, CHARLIE);

    await compare.goto(A, B, C);

    await expect(compare.heading).toBeVisible();
    await expect(compare.names).toHaveText([
      ALPHA.name,
      BRAVO.name,
      CHARLIE.name,
    ]);
    await expect(compare.prices).toHaveText([
      '$9.99',
      '$8.00 – $14.00',
      'Price unavailable',
    ]);
    await expect(compare.ratings).toHaveText([
      '4.5 (8 reviews)',
      '3.2 (1 review)',
      'No reviews yet',
    ]);
    await expect(compare.styles).toHaveText(['IPA, Hazy', 'Brown Ale', /—/]);
    expect(compare.detailRequests).toHaveLength(3);

    // One row per attribute and one cell per beer is what keeps the columns aligned.
    await expect(compare.table.locator('tbody tr')).toHaveCount(4);
    await expect(
      compare.table.locator('tbody tr').first().locator('td'),
    ).toHaveCount(3);
  });

  test('a beer the API no longer has keeps its column next to the others', async ({
    compare,
  }) => {
    await compare.mockBeer(A, ALPHA);
    await compare.mockBeer(B, 404);

    await compare.goto(A, B);

    await expect(compare.names).toHaveText([
      ALPHA.name,
      'This beer no longer exists.',
    ]);
    await expect(compare.ratings.first()).toContainText('4.5 (8 reviews)');
  });

  test('a single id shows the prompt without calling the API', async ({
    compare,
  }) => {
    await compare.goto(A);

    await expect(compare.prompt).toContainText(
      'Pick at least 2 different beers',
    );
    await expect(compare.heading).toBeVisible();
    await compare.waitForHydration();
    expect(compare.detailRequests).toHaveLength(0);
  });

  test('a link of junk shows the prompt without calling the API', async ({
    page,
    compare,
  }) => {
    await page.goto('/compare?beers=junk,123');

    await expect(compare.prompt).toBeVisible();
    await compare.waitForHydration();
    expect(compare.detailRequests).toHaveLength(0);
  });

  test('6 valid ids show the cap notice and fetch exactly 4 beers', async ({
    compare,
  }) => {
    for (const id of [A, B, C, D, E, F]) {
      await compare.mockBeer(id, { ...ALPHA, name: `Beer ${id.slice(-2)}` });
    }

    await compare.goto(A, B, C, D, E, F);

    await expect(compare.capped).toHaveText(
      'Comparisons are capped at 4 beers. Showing the first 4.',
    );
    await expect(compare.names).toHaveCount(4);
    expect(compare.detailRequests).toHaveLength(4);
    expect(compare.detailRequests.join()).not.toContain(E);
  });

  test('picking two beers and opening Compare lands on the shareable link without a reload', async ({
    page,
    compare,
    detail,
    explorer,
    shell,
  }) => {
    const mockBeers = [
      { beerId: A, name: 'Alpha Ale', styleId: STYLE_IPA, price: 6 },
      { beerId: B, name: 'Bravo Brown', styleId: STYLE_IPA, price: 7 },
    ];
    // The detail page refetches and clears the beer, and the button with it, when the GET fails.
    await compare.mockBeer(A, ALPHA);
    await compare.mockBeer(B, BRAVO);
    await explorer.mockApi({ beers: mockBeers });
    await explorer.goto();
    // A full-page load would reset the in-memory selection, so a marker proves there was none.
    await page.evaluate(() => {
      (globalThis as Record<string, unknown>)['spaMarker'] = true;
    });

    await expect(explorer.cards).toHaveCount(2);
    await explorer
      .card('Alpha Ale')
      .getByRole('link', { name: 'Alpha Ale' })
      .click();
    await expect(detail.compareToggle).toHaveText('Add to Compare');
    await detail.compareToggle.click();
    await expect(detail.compareToggle).toHaveText('Remove from Compare');
    await page.goBack();

    await expect(explorer.cards).toHaveCount(2);
    await explorer
      .card('Bravo Brown')
      .getByRole('link', { name: 'Bravo Brown' })
      .click();
    await expect(detail.compareToggle).toHaveText('Add to Compare');
    await detail.compareToggle.click();
    await expect(detail.compareToggle).toHaveText('Remove from Compare');
    await shell.navLink(shell.mainNavigation, 'Compare').click();

    await expect(page).toHaveURL(`/compare?beers=${A},${B}`);
    await expect(compare.names).toHaveText([ALPHA.name, BRAVO.name]);
    expect(
      await page.evaluate(
        () => (globalThis as Record<string, unknown>)['spaMarker'],
      ),
    ).toBe(true);
  });
});
