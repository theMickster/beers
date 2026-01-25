import type { Locator, Page, Route } from '@playwright/test';
import { toApiBrewer, type MockBrewer } from './brewer-directory.page';
import { BEER } from './beer-detail.page';

export const BREWER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
export const MISSING_BREWER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3399';

export const BREWER: MockBrewer = {
  brewerId: BREWER_ID,
  name: 'Odell Brewing',
  foundedIn: 1989,
  headquarters: 'Fort Collins, CO',
  website: 'https://odell.test',
  breweryType: { id: '3f2504e0-4f89-11d3-9a0c-0305e82c3391', name: 'Regional' },
  rating: { average: 4.25, reviewCount: 12 },
};

export interface MockBrewerBeer {
  readonly beerId: string;
  readonly name: string;
}

export interface ReviewInput {
  readonly reviewerName: string;
  readonly title: string;
  readonly comments: string;
  readonly rating: number;
}

export interface SearchCall {
  readonly pageNumber: string | null;
  readonly pageSize: string | null;
  readonly body: Record<string, unknown>;
}

const reviewGuid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c34${String(n).padStart(2, '0')}`;

/** `count` reviews in the order the API returns them, "Review 01" first. */
export function makeReviews(count: number, brewerId = BREWER_ID) {
  return Array.from({ length: count }, (_, i) => {
    const n = i + 1;
    return {
      reviewId: reviewGuid(n),
      brewerId,
      reviewerName: `Reviewer ${n}`,
      title: `Review ${String(n).padStart(2, '0')}`,
      comments: `Comments ${n}`,
      rating: (n % 5) + 1,
      isDeletable: false,
      createdDate: '2026-03-05T10:00:00Z',
      modifiedDate: '2026-03-05T10:00:00Z',
    };
  });
}

export class BrewerDetailPage {
  readonly heading: Locator;
  readonly loading: Locator;
  readonly notFound: Locator;
  readonly errorAlert: Locator;
  readonly type: Locator;
  readonly founded: Locator;
  readonly headquarters: Locator;
  readonly rating: Locator;
  readonly website: Locator;
  readonly beersHeading: Locator;
  readonly beers: Locator;
  readonly reviews: Locator;
  readonly reviewsEmpty: Locator;
  readonly pagination: Locator;
  readonly form: Locator;
  readonly violations: Locator;
  readonly submitError: Locator;
  readonly confirmation: Locator;

  /** Every API request for a brewer, its beers or its reviews, as `METHOD url`. */
  readonly apiRequests: string[] = [];
  /** The body and paging of every `POST /beers/search`. */
  readonly beerSearches: SearchCall[] = [];
  /** Every `POST /brewers/{id}/reviews/search`. */
  readonly reviewSearches: SearchCall[] = [];
  /** The body of every `POST /brewers/{id}/reviews`. */
  readonly createdReviews: unknown[] = [];

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { level: 1 });
    this.loading = page.getByTestId('loading');
    this.notFound = page.getByTestId('not-found');
    this.errorAlert = page.locator('.alert-error');
    this.type = page.getByTestId('brewer-type');
    this.founded = page.getByTestId('brewer-founded');
    this.headquarters = page.getByTestId('brewer-headquarters');
    this.rating = page.getByTestId('brewer-rating');
    this.website = page.getByTestId('brewer-website');
    this.beersHeading = page.getByRole('heading', { level: 2, name: /^Beers/ });
    this.beers = page.getByTestId('brewer-beer');
    this.reviews = page.getByTestId('review');
    this.reviewsEmpty = page.getByTestId('reviews-empty');
    this.pagination = page.getByRole('navigation', { name: 'Pagination' });
    this.form = page.getByTestId('review-form');
    this.violations = page.getByTestId('review-violations');
    this.submitError = page.getByTestId('review-submit-error');
    this.confirmation = page.getByTestId('review-confirmation');

    page.on('request', (request) => {
      const url = request.url();
      if (
        url.includes('/api/v1/brewers') ||
        url.includes('/api/v1/beers/search')
      ) {
        this.apiRequests.push(`${request.method()} ${url}`);
      }
    });
  }

  async mockBrewer(id: string, brewer: MockBrewer | number): Promise<void> {
    await this.page.route(`**/api/v1/brewers/${id}`, (route) =>
      typeof brewer === 'number'
        ? route.fulfill({ status: brewer, json: ['Mock failure.'] })
        : route.fulfill({ json: toApiBrewer(brewer) }),
    );
  }

  /** Answers `POST /beers/search`, which the brewer page uses to list the brewer's beers. */
  async mockBeers(beers: readonly MockBrewerBeer[]): Promise<void> {
    await this.page.route('**/api/v1/beers/search**', (route) => {
      const call = this.record(route, this.beerSearches);
      return route.fulfill({
        json: {
          pageNumber: 1,
          pageSize: Number(call.pageSize ?? 10),
          totalPages: 1,
          hasPreviousPage: false,
          hasNextPage: false,
          totalRecords: beers.length,
          results: beers.map((beer) => ({
            ...BEER,
            beerId: beer.beerId,
            name: beer.name,
            brewerId: BREWER_ID,
          })),
        },
      });
    });
  }

  /** Answers `POST /brewers/{id}/reviews/search` from `reviews`, honouring the page and size. */
  async mockReviews(
    id: string,
    reviews: ReturnType<typeof makeReviews>,
  ): Promise<void> {
    await this.page.route(
      `**/api/v1/brewers/${id}/reviews/search**`,
      (route) => {
        const call = this.record(route, this.reviewSearches);
        const size = Number(call.pageSize ?? 10);
        const number = Number(call.pageNumber ?? 1);
        const totalPages = Math.ceil(reviews.length / size);
        return route.fulfill({
          json: {
            pageNumber: number,
            pageSize: size,
            totalPages,
            hasPreviousPage: number > 1,
            hasNextPage: number < totalPages,
            totalRecords: reviews.length,
            results: reviews.slice((number - 1) * size, number * size),
          },
        });
      },
    );
  }

  /** Answers `POST /brewers/{id}/reviews`: with the saved review, or with `status` and `messages`. */
  async mockCreateReview(
    id: string,
    failure?: { status: number; messages: string[] },
  ): Promise<void> {
    await this.page.route(`**/api/v1/brewers/${id}/reviews`, (route) => {
      this.createdReviews.push(route.request().postDataJSON());
      return failure
        ? route.fulfill({ status: failure.status, json: failure.messages })
        : route.fulfill({
            status: 201,
            json: {
              ...makeReviews(1)[0],
              ...(route.request().postDataJSON() as ReviewInput),
            },
          });
    });
  }

  async goto(id: string, query = ''): Promise<void> {
    await this.page.goto(`/brewers/${id}${query}`);
  }

  review(title: string): Locator {
    return this.reviews.filter({ hasText: title });
  }

  async fillReview(input: ReviewInput): Promise<void> {
    await this.page.getByLabel('Your name').fill(input.reviewerName);
    await this.page.getByLabel('Title').fill(input.title);
    await this.page.getByLabel('Comments').fill(input.comments);
    await this.page
      .getByRole('radio', { name: String(input.rating), exact: true })
      .check();
  }

  async submitReview(): Promise<void> {
    await this.page.getByRole('button', { name: 'Submit review' }).click();
  }

  private record(route: Route, into: SearchCall[]): SearchCall {
    const url = new URL(route.request().url());
    const call: SearchCall = {
      pageNumber: url.searchParams.get('PageNumber'),
      pageSize: url.searchParams.get('PageSize'),
      body: (route.request().postDataJSON() ?? {}) as Record<string, unknown>,
    };
    into.push(call);
    return call;
  }
}
