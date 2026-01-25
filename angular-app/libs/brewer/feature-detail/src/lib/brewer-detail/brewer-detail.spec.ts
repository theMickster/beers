import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  type TestRequest,
} from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { BeersByBrewerStore, type Beer } from '@beers/beer/data-access';
import {
  BrewerDetailStore,
  BrewerReviewStore,
  type Brewer,
  type BrewerReview,
  type CreateBrewerReview,
} from '@beers/brewer/data-access';
import {
  API_BASE_URL,
  ApiError,
  type PagedResult,
} from '@beers/shared/data-access';
import { A, B, guid, makeBeer, makeBrewer, makeReview } from '../test-data';
import { BrewerDetailComponent } from './brewer-detail';

@Component({ template: '' })
class StubComponent {}

const ROUTES = [
  { path: 'brewers', component: StubComponent },
  { path: 'brewers/:brewerId', component: BrewerDetailComponent },
  { path: 'beers/:beerId', component: StubComponent },
];

function createFakeStores() {
  const reviews = {
    activeBrewerId: signal<string | null>(null),
    results: signal<readonly BrewerReview[]>([]),
    totalRecords: signal(0),
    totalPages: signal(0),
    error: signal<ApiError | null>(null),
    isPending: signal(false),
    search: vi.fn(),
    // Resolves when the test says so, and may set `error` first, as the real store does.
    create: vi.fn<
      (brewerId: string, review: CreateBrewerReview) => Promise<void>
    >(() => Promise.resolve()),
  };
  return {
    detail: {
      brewerId: signal<string | null>(null),
      brewer: signal<Brewer | null>(null),
      error: signal<ApiError | null>(null),
      notFound: signal(false),
      load: vi.fn(),
    },
    beers: {
      brewerId: signal<string | null>(null),
      beers: signal<readonly Beer[]>([]),
      totalRecords: signal(0),
      error: signal<ApiError | null>(null),
      isFulfilled: signal(false),
      load: vi.fn(),
    },
    reviews,
  };
}

describe('BrewerDetailComponent', () => {
  let stores: ReturnType<typeof createFakeStores>;
  let harness: RouterTestingHarness | undefined;
  let el: HTMLElement;

  /** The first visit creates the harness, so a test can set up the stores before the page exists. */
  async function visit(url = `/brewers/${A}`) {
    harness ??= await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    await settle();
    el = harness.routeNativeElement as HTMLElement;
  }

  async function settle() {
    if (!harness) throw new Error('visit a page first');
    await harness.fixture.whenStable();
    harness.detectChanges();
  }

  /** Makes every store hold a finished load for `id`, as if the component had asked for it. */
  function loaded(
    id: string,
    data: {
      brewer?: Brewer;
      beers?: Beer[];
      totalBeers?: number;
      reviews?: BrewerReview[];
      totalReviews?: number;
      totalPages?: number;
    } = {},
  ) {
    const reviews = data.reviews ?? [];
    stores.detail.brewerId.set(id);
    stores.detail.brewer.set(data.brewer ?? makeBrewer({ brewerId: id }));
    stores.beers.brewerId.set(id);
    stores.beers.beers.set(data.beers ?? []);
    stores.beers.totalRecords.set(data.totalBeers ?? data.beers?.length ?? 0);
    stores.beers.isFulfilled.set(true);
    stores.reviews.activeBrewerId.set(id);
    stores.reviews.results.set(reviews);
    stores.reviews.totalRecords.set(data.totalReviews ?? reviews.length);
    stores.reviews.totalPages.set(
      data.totalPages ?? (reviews.length > 0 ? 1 : 0),
    );
  }

  const query = <T extends Element>(selector: string) =>
    el.querySelector<T>(selector);
  const text = (node: Element | null | undefined) =>
    node?.textContent?.replace(/\s+/g, ' ').trim();
  const testId = (id: string) => query(`[data-testid="${id}"]`);
  const all = (id: string) => [...el.querySelectorAll(`[data-testid="${id}"]`)];
  const currentUrl = () => TestBed.inject(Router).url;
  const button = (name: string) =>
    [...el.querySelectorAll('button')].find(
      (b) => b.textContent?.trim() === name,
    );

  beforeEach(() => {
    harness = undefined;
    stores = createFakeStores();
    TestBed.configureTestingModule({
      providers: [
        provideRouter(ROUTES),
        { provide: BrewerDetailStore, useValue: stores.detail },
        { provide: BeersByBrewerStore, useValue: stores.beers },
        { provide: BrewerReviewStore, useValue: stores.reviews },
      ],
    });
  });

  describe('loading', () => {
    it('loads the brewer, its beers and page 1 of its reviews, by the lowercase id, once the browser has rendered', async () => {
      await visit(`/brewers/${A.toUpperCase()}`);

      expect(stores.detail.load).toHaveBeenCalledExactlyOnceWith(A);
      expect(stores.beers.load).toHaveBeenCalledExactlyOnceWith(A);
      expect(stores.reviews.search).toHaveBeenCalledExactlyOnceWith(
        A,
        { pageNumber: 1, pageSize: 10 },
        {},
      );
    });

    it('reloads only the reviews when the page changes', async () => {
      await visit();

      await visit(`/brewers/${A}?page=2`);

      expect(stores.reviews.search).toHaveBeenCalledTimes(2);
      expect(stores.reviews.search).toHaveBeenLastCalledWith(
        A,
        { pageNumber: 2, pageSize: 10 },
        {},
      );
      expect(stores.detail.load).toHaveBeenCalledTimes(1);
      expect(stores.beers.load).toHaveBeenCalledTimes(1);
    });

    it.each([
      ['not a number', 'abc'],
      ['zero', '0'],
      ['negative', '-3'],
      ['fractional', '1.5'],
    ])('reads a page that is %s as page 1', async (_, page) => {
      await visit(`/brewers/${A}?page=${page}`);

      expect(stores.reviews.search).toHaveBeenCalledWith(
        A,
        { pageNumber: 1, pageSize: 10 },
        {},
      );
    });

    it('does not reload when only an unrelated query param changes', async () => {
      await visit();

      await visit(`/brewers/${A}?utm=x`);

      expect(stores.reviews.search).toHaveBeenCalledTimes(1);
    });

    it('loads everything again for another brewer', async () => {
      await visit(`/brewers/${A}`);

      await visit(`/brewers/${B}`);

      expect(stores.detail.load).toHaveBeenLastCalledWith(B);
      expect(stores.beers.load).toHaveBeenLastCalledWith(B);
      expect(stores.reviews.search).toHaveBeenLastCalledWith(
        B,
        { pageNumber: 1, pageSize: 10 },
        {},
      );
    });

    it('asks only the brewer store for an id that is not a GUID, so it makes no beers or reviews request', async () => {
      await visit('/brewers/junk');

      expect(stores.detail.load).toHaveBeenCalledExactlyOnceWith('junk');
      expect(stores.beers.load).not.toHaveBeenCalled();
      expect(stores.reviews.search).not.toHaveBeenCalled();
    });
  });

  describe('while the brewer loads', () => {
    it('shows the loading state under one heading', async () => {
      await visit();

      const loading = testId('loading');
      expect(loading?.getAttribute('role')).toBe('status');
      expect(loading?.getAttribute('aria-busy')).toBe('true');
      expect(text(loading)).toContain('Loading brewer');
      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(text(query('h1'))).toBe('Brewer');
    });
  });

  describe('a brewer that does not exist', () => {
    beforeEach(() => {
      stores.detail.brewerId.set(A);
      stores.detail.notFound.set(true);
      stores.detail.error.set(
        new ApiError(404, ['Brewer not found.'], 'client'),
      );
    });

    it('shows not found with a way back to the directory, and no alert', async () => {
      await visit();

      const notFound = testId('not-found');
      expect(text(notFound)).toContain('Brewer not found');
      expect(notFound?.querySelector('a')?.getAttribute('href')).toBe(
        '/brewers',
      );
      expect(query('[role="alert"]')).toBeNull();
      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(text(query('h1'))).toBe('Brewer');
    });
  });

  describe('a failed brewer load', () => {
    beforeEach(() => {
      stores.detail.brewerId.set(A);
      stores.detail.error.set(
        new ApiError(500, ['The server encountered an error.'], 'server'),
      );
    });

    it('shows the message in an alert under one heading', async () => {
      await visit();

      expect(text(query('[role="alert"]'))).toContain(
        'The server encountered an error.',
      );
      expect(testId('not-found')).toBeNull();
      expect(el.querySelectorAll('h1')).toHaveLength(1);
    });

    it('asks for the same brewer again when told to try again', async () => {
      await visit();

      button('Try again')?.click();
      await settle();

      expect(stores.detail.load).toHaveBeenCalledTimes(2);
      expect(stores.detail.load).toHaveBeenLastCalledWith(A);
    });
  });

  // The stores outlive the component and never load before the first render, so a section must
  // not trust its store until the store says it was last asked for this route's brewer. A stable
  // test cannot see the transient first frame, so each gate is tested directly: the store holds
  // brewer A and does nothing when asked, and the route is brewer B.
  describe('the key gates', () => {
    beforeEach(() => {
      loaded(A, {
        brewer: makeBrewer({ brewerId: A, name: 'Alpha Brewing' }),
        beers: [makeBeer({ name: 'Alpha Ale' })],
        reviews: [makeReview({ title: 'Alpha review' })],
      });
    });

    it('shows loading, none of brewer A, under brewer B', async () => {
      await visit(`/brewers/${B}`);

      expect(testId('loading')).not.toBeNull();
      expect(el.textContent).not.toContain('Alpha');
    });

    it('does not show brewer A not-found under brewer B', async () => {
      stores.detail.notFound.set(true);
      stores.detail.error.set(new ApiError(404, ['x'], 'client'));

      await visit(`/brewers/${B}`);

      expect(testId('not-found')).toBeNull();
      expect(testId('loading')).not.toBeNull();
    });

    it('does not show brewer A error under brewer B', async () => {
      stores.detail.error.set(new ApiError(500, ['A failed.'], 'server'));

      await visit(`/brewers/${B}`);

      expect(query('[role="alert"]')).toBeNull();
      expect(el.textContent).not.toContain('A failed.');
    });

    it('shows the beers loading state, none of A beers, when only the beers store is behind', async () => {
      stores.detail.brewerId.set(B);
      stores.detail.brewer.set(
        makeBrewer({ brewerId: B, name: 'Bravo Brewing' }),
      );

      await visit(`/brewers/${B}`);

      expect(text(query('h1'))).toBe('Bravo Brewing');
      expect(testId('beers-loading')).not.toBeNull();
      expect(testId('brewer-beers')).toBeNull();
      expect(testId('beers-count')).toBeNull();
      expect(el.textContent).not.toContain('Alpha Ale');
    });

    it('does not show A beers error under B', async () => {
      stores.detail.brewerId.set(B);
      stores.detail.brewer.set(makeBrewer({ brewerId: B }));
      stores.beers.error.set(new ApiError(500, ['A beers failed.'], 'server'));

      await visit(`/brewers/${B}`);

      expect(testId('beers-error')).toBeNull();
      expect(testId('beers-loading')).not.toBeNull();
    });

    it('shows the reviews loading state, none of A reviews, when only the reviews store is behind', async () => {
      stores.detail.brewerId.set(B);
      stores.detail.brewer.set(makeBrewer({ brewerId: B }));

      await visit(`/brewers/${B}`);

      expect(testId('reviews-loading')).not.toBeNull();
      expect(testId('reviews-list')).toBeNull();
      expect(el.textContent).not.toContain('Alpha review');
    });

    it('does not show A reviews error under B', async () => {
      stores.detail.brewerId.set(B);
      stores.detail.brewer.set(makeBrewer({ brewerId: B }));
      stores.reviews.error.set(
        new ApiError(500, ['A reviews failed.'], 'server'),
      );

      await visit(`/brewers/${B}`);

      expect(testId('reviews-error')).toBeNull();
      expect(testId('reviews-loading')).not.toBeNull();
    });

    it('shows brewer A when the route is brewer A, even typed in uppercase', async () => {
      await visit(`/brewers/${A.toUpperCase()}`);

      expect(text(query('h1'))).toBe('Alpha Brewing');
      expect(text(testId('brewer-beer'))).toContain('Alpha Ale');
    });
  });

  describe('a loaded brewer', () => {
    it('shows the name as the only level-1 heading, with type, founded year, headquarters and rating', async () => {
      loaded(A);

      await visit();

      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(text(query('h1'))).toBe('Odell');
      expect(text(testId('brewer-type'))).toBe('Regional');
      expect(text(testId('brewer-founded'))).toBe('Founded 1989');
      expect(text(testId('brewer-headquarters'))).toBe('Fort Collins, CO');
      expect(text(testId('brewer-rating'))).toBe('4.3 (12 reviews)');
      expect(testId('loading')).toBeNull();
    });

    it('says a brewer with no rating has no reviews', async () => {
      loaded(A, { brewer: makeBrewer({ rating: null }) });

      await visit();

      expect(text(testId('brewer-rating'))).toBe('No reviews yet');
    });

    it('links to the website in a new tab without handing over the opener', async () => {
      loaded(A);

      await visit();

      const link = testId('brewer-website');
      expect(link?.getAttribute('href')).toBe('https://odell.test');
      expect(link?.getAttribute('target')).toBe('_blank');
      expect(link?.getAttribute('rel')).toBe('noopener noreferrer');
      expect(text(link)).toBe('Website (opens in a new tab)');
    });

    it('shows no website link when the brewer has none', async () => {
      loaded(A, { brewer: makeBrewer({ website: '' }) });

      await visit();

      expect(testId('brewer-website')).toBeNull();
    });

    it('links back to the directory', async () => {
      loaded(A);

      await visit();

      const back = [...el.querySelectorAll('a')].find((a) =>
        a.textContent?.includes('Back to Brewers'),
      );
      expect(back?.getAttribute('href')).toBe('/brewers');
    });
  });

  describe('the beers section', () => {
    it('lists each beer as a link with its type and rating, under a count', async () => {
      loaded(A, {
        beers: [
          makeBeer({
            beerId: guid(41),
            name: 'Hazy Day',
            beerType: { id: 't1', name: 'Ale' },
            rating: { average: 4, reviewCount: 3 },
          }),
          makeBeer({ beerId: guid(42), name: 'Dark Hour', rating: null }),
        ],
      });

      await visit();

      expect(text(testId('beers-count'))).toBe('(2)');
      const [first, second] = all('brewer-beer');
      expect(first.querySelector('a')?.getAttribute('href')).toBe(
        `/beers/${guid(41)}`,
      );
      expect(text(first.querySelector('a'))).toBe('Hazy Day');
      expect(text(first)).toContain('Ale');
      expect(text(first)).toContain('4.0 (3 reviews)');
      expect(text(second)).toContain('No reviews yet');
    });

    // The API caps a page at 50, so the real count must not be the list length.
    it('shows the real total, which exposes a truncated list', async () => {
      loaded(A, { beers: [makeBeer()], totalBeers: 61 });

      await visit();

      expect(text(testId('beers-count'))).toBe('(61)');
    });

    it('says there are no beers yet', async () => {
      loaded(A, { beers: [] });

      await visit();

      expect(text(testId('beers-empty'))).toBe('No beers yet');
      expect(text(testId('beers-count'))).toBe('(0)');
    });

    it('shows its own loading state while the beers load, with the brewer already on screen', async () => {
      loaded(A);
      stores.beers.isFulfilled.set(false);

      await visit();

      expect(text(testId('beers-loading'))).toContain('Loading beers');
      expect(text(query('h1'))).toBe('Odell');
    });

    it('shows its own error, leaves the rest of the page, and retries only the beers', async () => {
      loaded(A);
      stores.beers.isFulfilled.set(false);
      stores.beers.error.set(new ApiError(500, ['Beers failed.'], 'server'));
      await visit();

      expect(text(testId('beers-error'))).toContain('Beers failed.');
      expect(text(query('h1'))).toBe('Odell');

      testId('beers-error')?.querySelector('button')?.click();
      await settle();

      expect(stores.beers.load).toHaveBeenCalledTimes(2);
      expect(stores.beers.load).toHaveBeenLastCalledWith(A);
      expect(stores.detail.load).toHaveBeenCalledTimes(1);
    });
  });

  describe('the reviews section', () => {
    it('shows each review with title, reviewer, a rating as N out of 5, comments and date', async () => {
      loaded(A, {
        reviews: [
          makeReview({
            title: 'Great',
            reviewerName: 'Sam',
            rating: 4,
            comments: 'Loved the lager.',
            createdDate: '2026-03-05T10:00:00Z',
          }),
        ],
      });

      await visit();

      const review = testId('review');
      expect(text(review?.querySelector('h3'))).toBe('Great');
      expect(text(review)).toContain('Sam');
      expect(text(review?.querySelector('[data-testid="review-rating"]'))).toBe(
        '4 out of 5',
      );
      expect(text(review)).toContain('Loved the lager.');
      expect(text(review?.querySelector('time'))).toBe('Mar 5, 2026');
      expect(review?.querySelector('time')?.getAttribute('datetime')).toBe(
        '2026-03-05T10:00:00Z',
      );
    });

    it('offers no way to edit or delete a review', async () => {
      loaded(A, { reviews: [makeReview()] });

      await visit();

      expect(el.querySelectorAll('[data-testid="review"] button')).toHaveLength(
        0,
      );
      expect(el.querySelectorAll('[data-testid="review"] a')).toHaveLength(0);
    });

    it('says there are no reviews yet, with no pager', async () => {
      loaded(A, { reviews: [] });

      await visit();

      expect(text(testId('reviews-empty'))).toBe('No reviews yet');
      expect(query('nav[aria-label="Pagination"]')).toBeNull();
    });

    it('shows a pager for several pages, and a click puts the page in the URL', async () => {
      loaded(A, {
        reviews: [makeReview()],
        totalReviews: 25,
        totalPages: 3,
      });
      await visit();

      button('Next')?.click();
      await settle();

      expect(currentUrl()).toBe(`/brewers/${A}?page=2`);
      expect(stores.reviews.search).toHaveBeenLastCalledWith(
        A,
        { pageNumber: 2, pageSize: 10 },
        {},
      );
    });

    it('drops the page param when going back to page 1', async () => {
      loaded(A, { reviews: [makeReview()], totalReviews: 25, totalPages: 3 });
      await visit(`/brewers/${A}?page=2`);

      button('Previous')?.click();
      await settle();

      expect(currentUrl()).toBe(`/brewers/${A}`);
    });

    it('says there are no reviews on a page past the end, and keeps the pager', async () => {
      loaded(A, { reviews: [], totalReviews: 15, totalPages: 2 });

      await visit(`/brewers/${A}?page=9`);

      expect(text(testId('reviews-page-empty'))).toBe(
        'No reviews on this page',
      );
      expect(testId('reviews-empty')).toBeNull();
      expect(query('nav[aria-label="Pagination"]')).not.toBeNull();
    });

    it('shows a skeleton while a page loads, hiding the previous page', async () => {
      loaded(A, {
        reviews: [makeReview({ title: 'Page one' })],
        totalPages: 2,
      });
      stores.reviews.isPending.set(true);

      await visit();

      expect(text(testId('reviews-loading'))).toContain('Loading reviews');
      expect(el.textContent).not.toContain('Page one');
    });

    it('shows its own error with the brewer still on screen, and retries the same page', async () => {
      loaded(A);
      stores.reviews.error.set(
        new ApiError(500, ['Reviews failed.'], 'server'),
      );
      await visit(`/brewers/${A}?page=2`);

      expect(text(testId('reviews-error'))).toContain('Reviews failed.');
      expect(text(query('h1'))).toBe('Odell');

      testId('reviews-error')?.querySelector('button')?.click();
      await settle();

      expect(stores.reviews.search).toHaveBeenCalledTimes(2);
      expect(stores.reviews.search).toHaveBeenLastCalledWith(
        A,
        { pageNumber: 2, pageSize: 10 },
        {},
      );
    });
  });

  describe('the review form', () => {
    const form = () => query<HTMLFormElement>('[data-testid="review-form"]');
    const input = (name: string) =>
      query<HTMLInputElement | HTMLTextAreaElement>(`[name="${name}"]`);
    const submitButton = () => button('Submit review');

    function type(name: string, value: string) {
      const field = input(name);
      if (!field) throw new Error(`no field ${name}`);
      field.value = value;
      field.dispatchEvent(new Event('input'));
    }

    function chooseRating(value: number) {
      const radio = [
        ...el.querySelectorAll<HTMLInputElement>('[name="rating"]'),
      ].find((r) => r.value === String(value));
      if (!radio) throw new Error(`no rating ${value}`);
      radio.checked = true;
      radio.dispatchEvent(new Event('change'));
    }

    async function fillValidForm() {
      type('reviewerName', 'Sam');
      type('title', 'Great');
      type('comments', 'Loved the lager.');
      chooseRating(4);
      await settle();
    }

    const VALID: CreateBrewerReview = {
      reviewerName: 'Sam',
      title: 'Great',
      comments: 'Loved the lager.',
      rating: 4,
    };

    beforeEach(() =>
      loaded(A, { reviews: [makeReview({ title: 'Existing' })] }),
    );

    it('has the three text fields with their length limits and a 1 to 5 rating group', async () => {
      await visit();

      expect(form()?.getAttribute('novalidate')).not.toBeNull();
      expect(input('reviewerName')?.getAttribute('maxlength')).toBe('128');
      expect(input('title')?.getAttribute('maxlength')).toBe('256');
      expect(input('comments')?.tagName).toBe('TEXTAREA');
      expect(input('comments')?.getAttribute('maxlength')).toBe('4000');
      const radios = [
        ...el.querySelectorAll<HTMLInputElement>('[name="rating"]'),
      ];
      expect(radios.map((r) => r.value)).toEqual(['1', '2', '3', '4', '5']);
      expect(query('fieldset legend')?.textContent?.trim()).toBe('Rating');
    });

    describe('an invalid submit', () => {
      it('sends nothing and lists every broken rule, but only after the first attempt', async () => {
        await visit();
        expect(testId('review-violations')).toBeNull();

        submitButton()?.click();
        await settle();

        expect(stores.reviews.create).not.toHaveBeenCalled();
        const messages = [
          ...(testId('review-violations')?.querySelectorAll('li') ?? []),
        ].map(text);
        expect(messages).toEqual([
          'Reviewer name is required.',
          'Title is required.',
          'Comments is required.',
          'Rating must be a whole number from 1 to 5.',
        ]);
        expect(testId('review-violations')?.getAttribute('role')).toBe('alert');
      });

      it('updates the list as the user fixes the fields, and clears it once valid', async () => {
        await visit();
        submitButton()?.click();
        await settle();

        type('reviewerName', 'Sam');
        await settle();
        expect(text(testId('review-violations'))).not.toContain(
          'Reviewer name',
        );

        await fillValidForm();
        expect(testId('review-violations')).toBeNull();
      });
    });

    describe('a valid submit', () => {
      it('sends exactly the four fields for the brewer in the route and reloads the current page', async () => {
        await visit(`/brewers/${A}?page=2`);
        await fillValidForm();

        submitButton()?.click();
        await settle();

        expect(stores.reviews.create).toHaveBeenCalledExactlyOnceWith(A, VALID);
        expect(stores.reviews.search).toHaveBeenCalledTimes(2);
        expect(stores.reviews.search).toHaveBeenLastCalledWith(
          A,
          { pageNumber: 2, pageSize: 10 },
          {},
        );
      });

      it('clears the form and confirms', async () => {
        await visit();
        await fillValidForm();

        submitButton()?.click();
        await settle();

        expect(input('reviewerName')?.value).toBe('');
        expect(input('title')?.value).toBe('');
        expect(input('comments')?.value).toBe('');
        expect(
          el.querySelector<HTMLInputElement>('[name="rating"]:checked'),
        ).toBeNull();
        const confirmation = testId('review-confirmation');
        expect(confirmation?.getAttribute('role')).toBe('status');
        expect(text(confirmation)).toContain('your review was submitted');
        expect(testId('review-submit-error')).toBeNull();
      });

      it('does not show the broken rules from an earlier attempt after a successful one', async () => {
        await visit();
        submitButton()?.click();
        await settle();
        await fillValidForm();

        submitButton()?.click();
        await settle();

        expect(testId('review-violations')).toBeNull();
      });
    });

    describe('while the review is being saved', () => {
      let finish: () => void;

      beforeEach(() => {
        stores.reviews.create.mockImplementation(
          () => new Promise<void>((resolve) => (finish = resolve)),
        );
      });

      it('disables the button and keeps the list on screen even though the store is pending', async () => {
        await visit();
        await fillValidForm();

        submitButton()?.click();
        stores.reviews.isPending.set(true);
        await settle();

        expect(submitButton()?.disabled).toBe(true);
        expect(text(testId('reviews-list'))).toContain('Existing');
        expect(testId('reviews-loading')).toBeNull();
        finish();
      });

      it('does not show a failed create as the list error while it is still saving', async () => {
        await visit();
        await fillValidForm();

        submitButton()?.click();
        stores.reviews.error.set(new ApiError(400, ['Bad.'], 'client'));
        await settle();

        expect(testId('reviews-error')).toBeNull();
        expect(text(testId('reviews-list'))).toContain('Existing');
        finish();
      });

      it('sends one request for a double click', async () => {
        await visit();
        await fillValidForm();

        submitButton()?.click();
        submitButton()?.click();
        await settle();

        expect(stores.reviews.create).toHaveBeenCalledTimes(1);
        finish();
      });

      it('enables the button again once it is done', async () => {
        await visit();
        await fillValidForm();
        submitButton()?.click();
        await settle();

        finish();
        await settle();

        expect(submitButton()?.disabled).toBe(false);
      });

      // The review was saved for brewer A, but the page now shows B.
      it('neither clears the form nor confirms when the user moved to another brewer', async () => {
        await visit(`/brewers/${A}`);
        await fillValidForm();
        submitButton()?.click();
        await settle();
        const searchesBefore = stores.reviews.search.mock.calls.length;

        loaded(B);
        await visit(`/brewers/${B}`);
        finish();
        await settle();

        expect(input('title')?.value).toBe('Great');
        expect(input('reviewerName')?.value).toBe('Sam');
        expect(testId('review-confirmation')).toBeNull();
        expect(testId('review-submit-error')).toBeNull();
        // Only brewer B's own page load searched; the submit did not search again for A.
        expect(stores.reviews.search.mock.calls.slice(searchesBefore)).toEqual([
          [B, { pageNumber: 1, pageSize: 10 }, {}],
        ]);
        expect(submitButton()?.disabled).toBe(false);
      });
    });

    describe('a failed create', () => {
      beforeEach(() => {
        // The real store sets the status to pending before it requests, which clears the error.
        stores.reviews.search.mockImplementation(() =>
          stores.reviews.error.set(null),
        );
        stores.reviews.create.mockImplementation(() => {
          stores.reviews.error.set(
            new ApiError(400, ['Title is too spicy.', 'Try again.'], 'client'),
          );
          return Promise.resolve();
        });
      });

      it('shows the messages beside the form and keeps what the user typed', async () => {
        await visit();
        await fillValidForm();

        submitButton()?.click();
        await settle();

        const messages = [
          ...(testId('review-submit-error')?.querySelectorAll('li') ?? []),
        ].map(text);
        expect(messages).toEqual(['Title is too spicy.', 'Try again.']);
        expect(testId('review-submit-error')?.getAttribute('role')).toBe(
          'alert',
        );
        expect(input('title')?.value).toBe('Great');
        expect(input('comments')?.value).toBe('Loved the lager.');
        expect(testId('review-confirmation')).toBeNull();
      });

      it('keeps the list on screen and searches again so the list status is restored', async () => {
        await visit();
        await fillValidForm();

        submitButton()?.click();
        await settle();

        expect(stores.reviews.search).toHaveBeenCalledTimes(2);
        expect(text(testId('reviews-list'))).toContain('Existing');
      });

      it('clears the message on the next attempt', async () => {
        await visit();
        await fillValidForm();
        submitButton()?.click();
        await settle();
        stores.reviews.create.mockImplementation(() => Promise.resolve());
        stores.reviews.error.set(null);

        submitButton()?.click();
        await settle();

        expect(testId('review-submit-error')).toBeNull();
        expect(testId('review-confirmation')).not.toBeNull();
      });
    });

    it('does not carry a confirmation or a failure message over to another brewer', async () => {
      await visit(`/brewers/${A}`);
      await fillValidForm();
      submitButton()?.click();
      await settle();
      expect(testId('review-confirmation')).not.toBeNull();

      loaded(B);
      await visit(`/brewers/${B}`);

      expect(testId('review-confirmation')).toBeNull();
    });
  });
});

describe('BrewerDetailComponent with the real stores', () => {
  const BASE_URL = 'https://api.test/api/v1';
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
        provideRouter(ROUTES),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => http.verify());

  async function visit(url: string): Promise<HTMLElement> {
    await harness.navigateByUrl(url);
    await harness.fixture.whenStable();
    harness.detectChanges();
    return harness.routeNativeElement as HTMLElement;
  }

  const brewerRequest = (id: string) =>
    http.expectOne(`${BASE_URL}/brewers/${id}`);
  const beersRequest = () =>
    http.expectOne((r) => r.url === `${BASE_URL}/beers/search`);
  const reviewsRequest = (id: string) =>
    http.expectOne((r) => r.url === `${BASE_URL}/brewers/${id}/reviews/search`);

  function page<T>(
    results: T[],
    overrides: Partial<PagedResult<T>> = {},
  ): PagedResult<T> {
    return {
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: results.length,
      results,
      ...overrides,
    };
  }

  /** Answers the three requests a brewer page makes. */
  function answer(
    id: string,
    name: string,
    beerName: string,
    reviewTitle: string,
  ) {
    brewerRequest(id).flush(makeBrewer({ brewerId: id, name }));
    beersRequest().flush(
      page([makeBeer({ beerId: guid(70), brewerId: id, name: beerName })]),
    );
    reviewsRequest(id).flush(
      page([makeReview({ brewerId: id, title: reviewTitle })]),
    );
    harness.detectChanges();
  }

  it('asks for the brewer, its beers by brewerId and its first page of reviews', async () => {
    const el = await visit(`/brewers/${A}`);

    const beers = beersRequest();
    expect(beers.request.body).toEqual({ brewerId: A });
    expect(beers.request.params.get('PageSize')).toBe('50');
    const reviews = reviewsRequest(A);
    expect(reviews.request.params.get('PageNumber')).toBe('1');
    expect(reviews.request.params.get('PageSize')).toBe('10');
    expect(brewerRequest(A).request.method).toBe('GET');
    expect(el.querySelector('[data-testid="loading"]')).not.toBeNull();
    http.match(() => true).forEach((r) => r.flush(null));
  });

  it('never shows brewer A beers or reviews under brewer B', async () => {
    const first = await visit(`/brewers/${A}`);
    answer(A, 'Alpha Brewing', 'Alpha Ale', 'Alpha review');
    expect(first.querySelector('h1')?.textContent).toBe('Alpha Brewing');
    await visit('/brewers');

    const el = await visit(`/brewers/${B}`);

    const pending = [brewerRequest(B), beersRequest(), reviewsRequest(B)];
    expect(el.textContent).not.toContain('Alpha');
    expect(el.querySelector('[data-testid="loading"]')).not.toBeNull();

    // Brewer B answers first; its beers and reviews are still on the way.
    pending[0].flush(makeBrewer({ brewerId: B, name: 'Bravo Brewing' }));
    harness.detectChanges();
    expect(el.querySelector('h1')?.textContent).toBe('Bravo Brewing');
    expect(el.querySelector('[data-testid="beers-loading"]')).not.toBeNull();
    expect(el.querySelector('[data-testid="reviews-loading"]')).not.toBeNull();
    expect(el.textContent).not.toContain('Alpha');

    pending[1].flush(page([makeBeer({ brewerId: B, name: 'Bravo Brown' })]));
    pending[2].flush(
      page([makeReview({ brewerId: B, title: 'Bravo review' })]),
    );
    harness.detectChanges();
    expect(el.textContent).toContain('Bravo Brown');
    expect(el.textContent).toContain('Bravo review');
    expect(el.textContent).not.toContain('Alpha');
  });

  it('makes no brewer-scoped request but one for an id that is not a GUID, and shows not found', async () => {
    const el = await visit('/brewers/junk');

    http.expectNone(() => true);
    expect(el.querySelector('[data-testid="not-found"]')).not.toBeNull();
  });

  it('shows not found for a brewer the API does not have, while its beers and reviews still load', async () => {
    const el = await visit(`/brewers/${A}`);

    brewerRequest(A).flush(['Brewer not found.'], {
      status: 404,
      statusText: 'Not Found',
    });
    beersRequest().flush(page([]));
    reviewsRequest(A).flush(page([]));
    harness.detectChanges();

    expect(el.querySelector('[data-testid="not-found"]')).not.toBeNull();
    expect(el.querySelector('[role="alert"]')).toBeNull();
  });

  describe('submitting a review', () => {
    async function openLoaded(): Promise<HTMLElement> {
      const el = await visit(`/brewers/${A}`);
      answer(A, 'Alpha Brewing', 'Alpha Ale', 'Existing');
      return el;
    }

    function fill(el: HTMLElement) {
      const set = (name: string, value: string) => {
        const field = el.querySelector<HTMLInputElement>(`[name="${name}"]`);
        if (!field) throw new Error(name);
        field.value = value;
        field.dispatchEvent(new Event('input'));
      };
      set('reviewerName', 'Sam');
      set('title', 'Great');
      set('comments', 'Loved it.');
      const radio = el.querySelector<HTMLInputElement>(
        '[name="rating"][value="5"]',
      );
      if (!radio) throw new Error('rating');
      radio.checked = true;
      radio.dispatchEvent(new Event('change'));
    }

    const submit = (el: HTMLElement) =>
      [...el.querySelectorAll('button')]
        .find((b) => b.textContent?.trim() === 'Submit review')
        ?.click();

    /** The component awaits `create` as a promise, so its follow-up search starts a microtask later. */
    const afterCreate = async () => {
      await harness.fixture.whenStable();
      await new Promise((resolve) => setTimeout(resolve));
    };

    const createRequest = (): TestRequest =>
      http.expectOne(
        (r) =>
          r.method === 'POST' && r.url === `${BASE_URL}/brewers/${A}/reviews`,
      );

    it('posts the four fields, then reloads the reviews and shows the new one', async () => {
      const el = await openLoaded();
      fill(el);

      submit(el);
      await harness.fixture.whenStable();
      const create = createRequest();
      expect(create.request.body).toEqual({
        reviewerName: 'Sam',
        title: 'Great',
        comments: 'Loved it.',
        rating: 5,
      });
      create.flush(makeReview({ title: 'Great' }), {
        status: 201,
        statusText: 'Created',
      });
      await afterCreate();
      reviewsRequest(A).flush(
        page([
          makeReview({ title: 'Existing' }),
          makeReview({ reviewId: guid(61), title: 'Great' }),
        ]),
      );
      harness.detectChanges();

      expect(
        el.querySelector('[data-testid="review-confirmation"]'),
      ).not.toBeNull();
      expect(
        [...el.querySelectorAll('[data-testid="review"] h3')].map(
          (h) => h.textContent,
        ),
      ).toEqual(['Existing', 'Great']);
    });

    it('keeps the list on screen and shows the 400 messages beside the form when the create fails', async () => {
      const el = await openLoaded();
      fill(el);

      submit(el);
      await harness.fixture.whenStable();
      createRequest().flush(['Title is too spicy.'], {
        status: 400,
        statusText: 'Bad Request',
      });
      await afterCreate();
      reviewsRequest(A).flush(page([makeReview({ title: 'Existing' })]));
      harness.detectChanges();

      expect(
        el.querySelector('[data-testid="review-submit-error"]')?.textContent,
      ).toContain('Title is too spicy.');
      expect(el.querySelector('[data-testid="reviews-error"]')).toBeNull();
      expect(
        el.querySelector('[data-testid="reviews-list"]')?.textContent,
      ).toContain('Existing');
      expect(el.querySelector<HTMLInputElement>('[name="title"]')?.value).toBe(
        'Great',
      );
    });

    it('makes no request for an invalid form', async () => {
      const el = await openLoaded();

      submit(el);
      await harness.fixture.whenStable();

      http.expectNone(() => true);
      expect(
        el.querySelector('[data-testid="review-violations"]'),
      ).not.toBeNull();
    });
  });
});
