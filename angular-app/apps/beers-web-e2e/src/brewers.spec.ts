import { expect, test as base } from '@playwright/test';
import {
  BEER,
  BEER_ID,
  BREWER_ID as BEER_BREWER_ID,
  BeerDetailPage,
} from './page-objects/beer-detail.page';
import {
  BREWER,
  BREWER_ID,
  BrewerDetailPage,
  MISSING_BREWER_ID,
  makeReviews,
  type ReviewInput,
} from './page-objects/brewer-detail.page';
import {
  BREWERS,
  BrewerDirectoryPage,
  TYPE_MICRO,
  TYPE_REGIONAL,
  guid,
} from './page-objects/brewer-directory.page';

const test = base.extend<{
  directory: BrewerDirectoryPage;
  brewer: BrewerDetailPage;
  beerDetail: BeerDetailPage;
}>({
  directory: async ({ page }, use) => {
    await use(new BrewerDirectoryPage(page));
  },
  brewer: async ({ page }, use) => {
    await use(new BrewerDetailPage(page));
  },
  beerDetail: async ({ page }, use) => {
    await use(new BeerDetailPage(page));
  },
});

test.use({ viewport: { width: 1280, height: 900 } });

const BEERS = [
  { beerId: guid(41), name: 'Hazy Day' },
  { beerId: guid(42), name: 'Dark Hour' },
];

const VALID_REVIEW: ReviewInput = {
  reviewerName: 'Sam',
  title: 'Great lager',
  comments: 'Crisp and clean.',
  rating: 4,
};

test.describe('brewer directory', () => {
  test('server renders only the loading state; the browser then loads the brewers', async ({
    page,
    directory,
  }) => {
    await directory.mockApi();
    const html = await (await page.request.get('/brewers')).text();

    expect(html).toContain('data-testid="loading"');
    expect(html).not.toContain('data-testid="brewer-card"');

    await directory.goto();
    await expect(directory.heading).toBeVisible();
    await expect(directory.cards).toHaveCount(6);
    expect(directory.listRequests).toHaveLength(1);
  });

  test('pages 10 brewers as 6 then 4, sorted by name, with the page in the URL', async ({
    page,
    directory,
  }) => {
    await directory.mockApi();
    await directory.goto();
    await expect(directory.cards).toHaveCount(6);
    await expect(directory.cards.first()).toContainText('Brewer 01');

    await directory.pageButton('Page 2').click();

    await expect(page).toHaveURL('/brewers?page=2');
    await expect(directory.cards).toHaveCount(4);
    await expect(directory.cards.first()).toContainText('Brewer 07');
    // The whole list was fetched once; paging is on the client.
    expect(directory.listRequests).toHaveLength(1);
  });

  test('a card shows the name link, founded year, headquarters, type and rating', async ({
    directory,
  }) => {
    await directory.mockApi();
    await directory.goto();

    const card = directory.card('Brewer 01');
    await expect(card.getByRole('link', { name: 'Brewer 01' })).toHaveAttribute(
      'href',
      `/brewers/${BREWERS[0].brewerId}`,
    );
    await expect(card).toContainText('Founded 1981');
    await expect(card).toContainText('Town 1, CO');
    await expect(card).toContainText('Micro');
    await expect(card).toContainText('4.5 (3 reviews)');
    await expect(directory.card('Brewer 02')).toContainText('No reviews yet');
  });

  test('the type select narrows the list and the URL carries the type', async ({
    page,
    directory,
  }) => {
    await directory.mockApi();
    await directory.goto('?page=2');
    await expect(directory.cards).toHaveCount(4);
    await expect(directory.typeSelect.locator('option')).toHaveText([
      'All types',
      'Micro',
      'Regional',
    ]);

    await directory.typeSelect.selectOption({ label: 'Regional' });

    // The page is reset, so the list starts at its first page.
    await expect(page).toHaveURL(`/brewers?type=${TYPE_REGIONAL.id}`);
    await expect(directory.cards).toHaveCount(4);
    await expect(directory.cards.first()).toContainText('Brewer 07');
    await expect(directory.pagination).toHaveCount(0);

    await directory.typeSelect.selectOption({ label: 'All types' });
    await expect(directory.cards).toHaveCount(6);
  });

  test('a shared link with a type reads the same list', async ({
    directory,
  }) => {
    await directory.mockApi();
    await directory.goto(`?type=${TYPE_MICRO.id}`);

    await expect(directory.cards).toHaveCount(6);
    await expect(directory.typeSelect).toHaveValue(TYPE_MICRO.id);
  });

  test('a type no brewer has and a page past the end fall back to valid values', async ({
    page,
    directory,
  }) => {
    await directory.mockApi();
    await directory.goto(`?type=${guid(99)}&page=99`);

    await expect(directory.cards).toHaveCount(4);
    await expect(directory.typeSelect).toHaveValue('');
    await expect(
      directory.pagination.locator('[aria-current="page"]'),
    ).toHaveAttribute('aria-label', 'Page 2');
    await expect(page).toHaveURL(`/brewers?type=${guid(99)}&page=99`);

    await directory.goto('?type=junk&page=abc');
    await expect(directory.cards).toHaveCount(6);
    await expect(directory.typeSelect).toHaveValue('');
  });

  test('a server failure shows an error with a retry', async ({
    directory,
  }) => {
    await directory.mockApi(BREWERS, 500);
    await directory.goto();

    await expect(directory.errorAlert).toBeVisible();
    await expect(
      directory.errorAlert.getByRole('button', { name: 'Try again' }),
    ).toBeVisible();
    await expect(directory.cards).toHaveCount(0);
  });
});

test.describe('brewer detail', () => {
  test('server renders only the loading state; the browser then loads the brewer', async ({
    page,
    brewer,
  }) => {
    await brewer.mockBrewer(BREWER_ID, BREWER);
    const html = await (await page.request.get(`/brewers/${BREWER_ID}`)).text();

    expect(html).toContain('data-testid="loading"');
    expect(html).not.toContain(BREWER.name);

    await brewer.mockBeers(BEERS);
    await brewer.mockReviews(BREWER_ID, makeReviews(0));
    await brewer.goto(BREWER_ID);
    await expect(brewer.heading).toHaveText(BREWER.name);
  });

  test('shows the brewer, its beers, its website and 10 reviews with a second page', async ({
    page,
    brewer,
  }) => {
    await brewer.mockBrewer(BREWER_ID, BREWER);
    await brewer.mockBeers(BEERS);
    await brewer.mockReviews(BREWER_ID, makeReviews(14));

    await brewer.goto(BREWER_ID);

    await expect(brewer.heading).toHaveText(BREWER.name);
    await expect(brewer.type).toHaveText('Regional');
    await expect(brewer.founded).toHaveText('Founded 1989');
    await expect(brewer.headquarters).toHaveText('Fort Collins, CO');
    await expect(brewer.rating).toContainText('4.3 (12 reviews)');
    await expect(brewer.website).toHaveAttribute('href', 'https://odell.test');
    await expect(brewer.website).toHaveAttribute('target', '_blank');
    await expect(brewer.website).toHaveAttribute('rel', 'noopener noreferrer');

    await expect(brewer.beersHeading).toContainText('(2)');
    await expect(brewer.beers).toHaveCount(2);
    await expect(
      brewer.beers.first().getByRole('link', { name: 'Hazy Day' }),
    ).toHaveAttribute('href', `/beers/${BEERS[0].beerId}`);
    expect(brewer.beerSearches).toHaveLength(1);
    expect(brewer.beerSearches[0].body).toEqual({ brewerId: BREWER_ID });
    expect(brewer.beerSearches[0].pageSize).toBe('50');

    await expect(brewer.reviews).toHaveCount(10);
    await expect(brewer.reviews.first()).toContainText('Review 01');
    await expect(brewer.reviews.first()).toContainText('Reviewer 1');
    await expect(brewer.reviews.first()).toContainText('Comments 1');
    await expect(brewer.reviews.first()).toContainText('2 out of 5');
    await expect(brewer.reviews.first()).toContainText('Mar 5, 2026');

    await brewer.pagination.getByRole('button', { name: 'Page 2' }).click();

    await expect(page).toHaveURL(`/brewers/${BREWER_ID}?page=2`);
    await expect(brewer.reviews).toHaveCount(4);
    await expect(brewer.reviews.first()).toContainText('Review 11');
    // A page click reloads only the reviews.
    expect(brewer.beerSearches).toHaveLength(1);
    expect(brewer.reviewSearches.map((call) => call.pageNumber)).toEqual([
      '1',
      '2',
    ]);
  });

  test('says so when the brewer has no beers or reviews', async ({
    brewer,
  }) => {
    await brewer.mockBrewer(BREWER_ID, BREWER);
    await brewer.mockBeers([]);
    await brewer.mockReviews(BREWER_ID, makeReviews(0));

    await brewer.goto(BREWER_ID);

    await expect(brewer.heading).toHaveText(BREWER.name);
    await expect(brewer.beersHeading).toContainText('(0)');
    await expect(brewer.reviewsEmpty).toHaveText('No reviews yet');
  });

  test('an id that is not a GUID shows not found without a brewer, beers or reviews request', async ({
    brewer,
  }) => {
    await brewer.goto('not-a-guid');

    await expect(brewer.notFound).toContainText('Brewer not found');
    await expect(brewer.heading).toHaveText('Brewer');
    await expect(brewer.errorAlert).toHaveCount(0);
    expect(brewer.apiRequests).toEqual([]);
  });

  test('a brewer the API does not have shows not found', async ({ brewer }) => {
    await brewer.mockBrewer(MISSING_BREWER_ID, 404);
    await brewer.mockBeers([]);
    await brewer.mockReviews(MISSING_BREWER_ID, makeReviews(0));

    await brewer.goto(MISSING_BREWER_ID);

    await expect(brewer.notFound).toContainText('Brewer not found');
    await expect(brewer.errorAlert).toHaveCount(0);
  });

  test('submitting a valid review posts exactly the four fields and shows a confirmation', async ({
    brewer,
  }) => {
    await brewer.mockBrewer(BREWER_ID, BREWER);
    await brewer.mockBeers(BEERS);
    await brewer.mockReviews(BREWER_ID, makeReviews(3));
    await brewer.mockCreateReview(BREWER_ID);
    await brewer.goto(BREWER_ID);
    // The form renders only once Angular can handle its submit.
    await expect(brewer.form).toBeVisible();
    await expect(brewer.reviews).toHaveCount(3);

    await brewer.fillReview(VALID_REVIEW);
    await brewer.submitReview();

    await expect(brewer.confirmation).toBeVisible();
    expect(brewer.createdReviews).toEqual([VALID_REVIEW]);
    // The list is fetched again so the new review can show.
    await expect.poll(() => brewer.reviewSearches.length).toBe(2);
  });

  test('an invalid submit lists the broken rules and makes no POST', async ({
    brewer,
  }) => {
    await brewer.mockBrewer(BREWER_ID, BREWER);
    await brewer.mockBeers(BEERS);
    await brewer.mockReviews(BREWER_ID, makeReviews(3));
    await brewer.mockCreateReview(BREWER_ID);
    await brewer.goto(BREWER_ID);
    await expect(brewer.form).toBeVisible();

    await brewer.submitReview();

    await expect(brewer.violations).toContainText('Reviewer name is required.');
    await expect(brewer.violations).toContainText(
      'Rating must be a whole number from 1 to 5.',
    );
    expect(brewer.createdReviews).toEqual([]);
    await expect(brewer.confirmation).toHaveCount(0);
  });

  test('a failed create keeps the reviews on screen, shows the 400 messages beside the form and keeps what was typed', async ({
    page,
    brewer,
  }) => {
    await brewer.mockBrewer(BREWER_ID, BREWER);
    await brewer.mockBeers(BEERS);
    await brewer.mockReviews(BREWER_ID, makeReviews(10));
    await brewer.mockCreateReview(BREWER_ID, {
      status: 400,
      messages: ['Title is too spicy.', 'Comments are too bland.'],
    });
    await brewer.goto(BREWER_ID);
    await expect(brewer.form).toBeVisible();
    await expect(brewer.reviews).toHaveCount(10);

    await brewer.fillReview(VALID_REVIEW);
    await brewer.submitReview();

    await expect(brewer.submitError).toContainText('Title is too spicy.');
    await expect(brewer.submitError).toContainText('Comments are too bland.');
    await expect(brewer.reviews).toHaveCount(10);
    await expect(brewer.errorAlert).toHaveCount(1);
    await expect(brewer.confirmation).toHaveCount(0);
    await expect(page.getByLabel('Title')).toHaveValue(VALID_REVIEW.title);
    expect(brewer.createdReviews).toHaveLength(1);
  });

  test('the beer page brewer link opens that brewer', async ({
    page,
    beerDetail,
    brewer,
  }) => {
    await beerDetail.mockBeer(BEER_ID);
    await brewer.mockBrewer(BEER_BREWER_ID, {
      ...BREWER,
      brewerId: BEER_BREWER_ID,
      name: BEER.brewer.name,
    });
    await brewer.mockBeers(BEERS);
    await brewer.mockReviews(BEER_BREWER_ID, makeReviews(0, BEER_BREWER_ID));
    await beerDetail.goto(BEER_ID);
    await expect(beerDetail.heading).toHaveText(BEER.name);

    await beerDetail.brewerLink.click();

    await expect(page).toHaveURL(`/brewers/${BEER_BREWER_ID}`);
    await expect(brewer.heading).toHaveText(BEER.brewer.name);
    await expect(brewer.beers).toHaveCount(2);
  });
});
