import { expect, test as base } from '@playwright/test';
import {
  BEER,
  BEER_ID,
  BREWER_ID,
  BeerDetailPage,
  MISSING_BEER_ID,
} from './page-objects/beer-detail.page';

const test = base.extend<{ detail: BeerDetailPage }>({
  detail: async ({ page }, use) => {
    await use(new BeerDetailPage(page));
  },
});

test.use({ viewport: { width: 1280, height: 900 } });

test.describe('beer detail', () => {
  test('server renders only the loading state; the browser then loads the beer', async ({
    page,
    detail,
  }) => {
    await detail.mockBeer(BEER_ID);
    const response = await page.request.get(`/beers/${BEER_ID}`);
    const html = await response.text();

    // The beer name would be in the HTML only if the server had fetched and rendered it.
    expect(html).toContain('data-testid="loading"');
    expect(html).not.toContain(BEER.name);

    await detail.goto(BEER_ID);
    await expect(detail.heading).toHaveText(BEER.name);
    expect(detail.detailRequests).toHaveLength(1);
  });

  test('shows the description, every pricing tier, the rating and a brewer link', async ({
    page,
    detail,
  }) => {
    await detail.mockBeer(BEER_ID);
    await detail.goto(BEER_ID);

    await expect(detail.heading).toHaveText(BEER.name);
    await expect(page.getByText(BEER.description)).toBeVisible();
    await expect(detail.rating).toContainText('4.3 (12 reviews)');
    await expect(detail.pricingRows).toHaveCount(2);
    await expect(detail.pricingRows.nth(0)).toContainText('Can');
    await expect(detail.pricingRows.nth(0)).toContainText('$9.99');
    await expect(detail.pricingRows.nth(1)).toContainText('Bottle');
    await expect(detail.pricingRows.nth(1)).toContainText('$18.00');
    await expect(detail.brewerLink).toHaveText('Odell');
    await expect(detail.brewerLink).toHaveAttribute(
      'href',
      `/brewers/${BREWER_ID}`,
    );
  });

  test('an id that is not a GUID shows not found without calling the API', async ({
    detail,
  }) => {
    await detail.goto('not-a-guid');

    await expect(detail.notFound).toContainText('Beer not found');
    await expect(detail.errorAlert).toHaveCount(0);
    expect(detail.detailRequests).toHaveLength(0);
  });

  test('a beer the API does not have shows not found', async ({ detail }) => {
    await detail.mockBeer(MISSING_BEER_ID, 404);
    await detail.goto(MISSING_BEER_ID);

    await expect(detail.notFound).toContainText('Beer not found');
    await expect(detail.errorAlert).toHaveCount(0);
    expect(detail.detailRequests).toHaveLength(1);
  });

  test('a server failure shows an error with a retry, not not-found', async ({
    detail,
  }) => {
    await detail.mockBeer(BEER_ID, 500);
    await detail.goto(BEER_ID);

    await expect(detail.errorAlert).toBeVisible();
    await expect(detail.notFound).toHaveCount(0);
    await expect(
      detail.errorAlert.getByRole('button', { name: 'Try again' }),
    ).toBeVisible();
  });

  test('Add to Compare toggles between add and remove', async ({ detail }) => {
    await detail.mockBeer(BEER_ID);
    await detail.goto(BEER_ID);

    await expect(detail.compareToggle).toHaveText('Add to Compare');
    await detail.compareToggle.click();
    await expect(detail.compareToggle).toHaveText('Remove from Compare');
    await detail.compareToggle.click();
    await expect(detail.compareToggle).toHaveText('Add to Compare');
  });
});
