import { expect, test as base } from '@playwright/test';
import {
  ExplorerPage,
  STYLE_IPA,
  STYLE_STOUT,
  TYPE_ALE,
} from './page-objects/explorer.page';

const test = base.extend<{ explorer: ExplorerPage }>({
  explorer: async ({ page }, use) => {
    await use(new ExplorerPage(page));
  },
});

test.use({ viewport: { width: 1280, height: 900 } });

test.describe('explorer', () => {
  test('server renders only the loading state; the browser then loads beers', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    const response = await page.request.get('/explorer');
    const html = await response.text();

    expect(html).toContain('data-testid="loading"');
    expect(html).not.toContain('<bw-beer-card');
    expect(explorer.searches).toHaveLength(0);

    await explorer.goto();
    await expect(explorer.cards).toHaveCount(12);
    expect(explorer.searches).toHaveLength(1);
  });

  test('shows a card with brewer, rating, price and a detail link', async ({
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto();

    const card = explorer.card('Beer 01');
    await expect(explorer.heading).toBeVisible();
    await expect(card).toContainText('Mock Brewing');
    await expect(card).toContainText('4.5 (3 reviews)');
    await expect(card).toContainText('$6.00');
    await expect(card.getByRole('link', { name: 'Beer 01' })).toHaveAttribute(
      'href',
      '/beers/beer-01',
    );
    await expect(explorer.resultCount).toHaveText('15 beers');
  });

  test('a style filter updates the URL, the request and the grid', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto();
    await expect(explorer.cards).toHaveCount(12);

    await explorer.checkbox('Stout').check();

    await expect(page).toHaveURL(`/explorer?style=${STYLE_STOUT}`);
    await expect(explorer.cards).toHaveCount(7);
    await expect(explorer.resultCount).toHaveText('7 beers');
    expect(explorer.searches.at(-1)?.body).toEqual({
      beerStyleIds: [STYLE_STOUT],
    });

    await explorer.checkbox('IPA').check();
    await expect(page).toHaveURL(
      `/explorer?style=${STYLE_STOUT}&style=${STYLE_IPA}`,
    );
    await expect(explorer.cards).toHaveCount(12);
  });

  test('a price range goes to the server and the URL', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto();
    // Cards appear only after hydration. A submit before then is a native form submit that
    // reloads the page, because the handler's preventDefault cannot run during event replay.
    await expect(explorer.cards).toHaveCount(12);

    await explorer.minPrice.fill('10');
    await explorer.maxPrice.fill('14');
    await explorer.applyPrice.click();

    await expect(page).toHaveURL('/explorer?minPrice=10&maxPrice=14');
    await expect(explorer.resultCount).toHaveText('7 beers');
    expect(explorer.searches.at(-1)?.body).toEqual({
      minPrice: 10,
      maxPrice: 14,
    });
  });

  test('blocks a price range where min is greater than max', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto();
    await expect(explorer.cards).toHaveCount(12);
    const before = explorer.searches.length;

    await explorer.minPrice.fill('20');
    await explorer.maxPrice.fill('5');

    await expect(explorer.priceError).toContainText(
      'Minimum price cannot be greater than maximum price',
    );
    await expect(explorer.applyPrice).toBeDisabled();
    await expect(page).toHaveURL('/explorer');
    expect(explorer.searches).toHaveLength(before);
  });

  test('restores filters, page and page size from the URL', async ({
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto(
      `?style=${STYLE_IPA}&type=${TYPE_ALE}&minPrice=5&pageSize=24&page=1`,
    );

    await expect(explorer.checkbox('IPA')).toBeChecked();
    await expect(explorer.checkbox('Stout')).not.toBeChecked();
    await expect(explorer.checkbox('Ale')).toBeChecked();
    await expect(explorer.minPrice).toHaveValue('5');
    await expect(explorer.pageSize).toHaveValue('24');
    await expect(explorer.cards).toHaveCount(8);
    expect(explorer.searches.at(-1)).toEqual({
      pageNumber: '1',
      pageSize: '24',
      body: { beerStyleIds: [STYLE_IPA], beerTypeIds: [TYPE_ALE], minPrice: 5 },
    });
  });

  test('drops invalid URL params instead of sending them to the API', async ({
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto(
      '?style=not-a-guid&minPrice=-1&maxPrice=abc&page=0&pageSize=999',
    );

    await expect(explorer.cards).toHaveCount(12);
    expect(explorer.searches).toEqual([
      { pageNumber: '1', pageSize: '12', body: {} },
    ]);
  });

  test('pages through the results and restores them with back and forward', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto();
    await expect(explorer.cards).toHaveCount(12);

    await explorer.pageButton('Next').click();
    await expect(page).toHaveURL('/explorer?page=2');
    await expect(explorer.cards).toHaveCount(3);
    await expect(explorer.pageButton('Page 2')).toHaveAttribute(
      'aria-current',
      'page',
    );
    await expect(explorer.pageButton('Next')).toBeDisabled();

    await page.goBack();
    await expect(page).toHaveURL('/explorer');
    await expect(explorer.cards).toHaveCount(12);

    await page.goForward();
    await expect(explorer.cards).toHaveCount(3);
  });

  test('changing the page size resets to the first page', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto('?page=2');
    await expect(explorer.cards).toHaveCount(3);

    await explorer.pageSize.selectOption('24');

    await expect(page).toHaveURL('/explorer?pageSize=24');
    await expect(explorer.cards).toHaveCount(15);
    await expect(explorer.pagination).toBeHidden();
  });

  test('shows the empty state when nothing matches and clears from it', async ({
    page,
    explorer,
  }) => {
    await explorer.mockApi();
    await explorer.goto('?minPrice=100');

    await expect(explorer.emptyState).toContainText('No beers match');
    await expect(explorer.cards).toHaveCount(0);

    await explorer.emptyState
      .getByRole('button', { name: 'Clear filters' })
      .click();
    await expect(page).toHaveURL('/explorer');
    await expect(explorer.cards).toHaveCount(12);
  });

  test('shows an error state when the search fails', async ({ explorer }) => {
    await explorer.mockApi({ searchStatus: 500 });
    await explorer.goto();

    await expect(explorer.errorAlert).toBeVisible();
    await expect(explorer.emptyState).toHaveCount(0);
    await expect(
      explorer.errorAlert.getByRole('button', { name: 'Try again' }),
    ).toBeVisible();
  });
});
