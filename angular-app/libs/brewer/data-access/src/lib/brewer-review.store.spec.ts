import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  type TestRequest,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  API_BASE_URL,
  ApiError,
  type PagedResult,
} from '@beers/shared/data-access';
import type {
  BrewerReview,
  BrewerReviewPageQuery,
  BrewerReviewSearchFilters,
  CreateBrewerReview,
} from './brewer-review.models';
import { BrewerReviewApiService } from './brewer-review-api.service';
import { BrewerReviewStore } from './brewer-review.store';

const BASE_URL = 'https://api.test/api/v1';
const BREWER_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BREWER_B = '9b2c4a10-1f5e-4c1d-8a3b-7d6e5f4a3b21';
const R1 = '00000000-0000-0000-0000-000000000001';
const R2 = '00000000-0000-0000-0000-000000000002';
const R3 = '00000000-0000-0000-0000-000000000003';
const REVIEW_WITH_LETTERS = 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d';

const reviewsUrl = (brewerId: string) =>
  `${BASE_URL}/brewers/${brewerId}/reviews`;
const searchUrl = (brewerId: string) => `${reviewsUrl(brewerId)}/search`;

function makeReview(
  reviewId: string,
  brewerId = BREWER_A,
  title = `Review ${reviewId}`,
): BrewerReview {
  return {
    reviewId,
    brewerId,
    reviewerName: 'Sam',
    title,
    comments: 'Nice',
    rating: 4,
    isDeletable: true,
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

function makePage(
  reviews: BrewerReview[],
  overrides: Partial<PagedResult<BrewerReview>> = {},
): PagedResult<BrewerReview> {
  return {
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    totalRecords: reviews.length,
    results: reviews,
    ...overrides,
  };
}

const writePayload: CreateBrewerReview = {
  reviewerName: 'Sam',
  title: 'Great',
  comments: 'Would visit again',
  rating: 5,
};

describe('BrewerReviewStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BrewerReviewStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BrewerReviewStore);
  });

  afterEach(() => httpMock.verify());

  const idsOf = () => store.ids().map(String);
  const resultIds = () => store.results().map((review) => review.reviewId);

  function expectSearch(brewerId = BREWER_A): TestRequest {
    return httpMock.expectOne((r) => r.url === searchUrl(brewerId));
  }

  const notFound = { status: 404, statusText: 'Not Found' };
  const serverError = { status: 500, statusText: 'Server Error' };

  it('starts idle, empty and without an active brewer', () => {
    expect(store.isIdle()).toBe(true);
    expect(store.error()).toBeNull();
    expect(store.activeBrewerId()).toBeNull();
    expect(store.entities()).toEqual([]);
    expect(store.results()).toEqual([]);
    expect(store.totalRecords()).toBe(0);
  });

  describe('list', () => {
    it('replaces entities, keeps the paged results untouched and sets the active brewer', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)]));

      store.list(BREWER_A);
      expect(store.isPending()).toBe(true);
      httpMock
        .expectOne(reviewsUrl(BREWER_A))
        .flush([makeReview(R2), makeReview(R3)]);

      expect(store.isFulfilled()).toBe(true);
      expect(store.activeBrewerId()).toBe(BREWER_A);
      expect(idsOf()).toEqual([R2, R3]);
      expect(resultIds()).toEqual([R1]);
      expect(store.totalRecords()).toBe(1);
    });

    it('is empty and fulfilled on a 404', () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush('None', notFound);

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.entities()).toEqual([]);
    });

    it('sets a server ApiError on 500 and recovers on the next call', () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush('boom', serverError);
      expect(store.error()).toBeInstanceOf(ApiError);
      expect(store.error()?.kind).toBe('server');

      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([R1]);
    });

    it('sets a network ApiError on status 0', () => {
      store.list(BREWER_A);
      httpMock
        .expectOne(reviewsUrl(BREWER_A))
        .error(new ProgressEvent('error'));
      expect(store.error()?.kind).toBe('network');
    });

    it('records an invalid brewer id as an error without a request', () => {
      store.list('not-a-guid');

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('search', () => {
    it('fills results, paging metadata and entities', () => {
      store.search(BREWER_A, { pageNumber: 1, pageSize: 2 }, { name: 'ipa' });
      expect(store.isPending()).toBe(true);

      const request = expectSearch();
      expect(request.request.params.get('PageSize')).toBe('2');
      expect(request.request.body).toEqual({ name: 'ipa' });
      request.flush(
        makePage([makeReview(R1), makeReview(R2)], {
          pageSize: 2,
          totalPages: 2,
          totalRecords: 3,
          hasNextPage: true,
        }),
      );

      expect(store.isFulfilled()).toBe(true);
      expect(resultIds()).toEqual([R1, R2]);
      expect(store.totalRecords()).toBe(3);
      expect(store.totalPages()).toBe(2);
      expect(store.hasNextPage()).toBe(true);
      expect(idsOf()).toEqual([R1, R2]);
    });

    it('upserts a second page without duplicating entities', () => {
      store.search(BREWER_A, { pageNumber: 1 }, {});
      expectSearch().flush(makePage([makeReview(R1), makeReview(R2)]));

      store.search(BREWER_A, { pageNumber: 2 }, {});
      expectSearch().flush(
        makePage([makeReview(R2, BREWER_A, 'Renamed'), makeReview(R3)], {
          pageNumber: 2,
        }),
      );

      expect(idsOf()).toEqual([R1, R2, R3]);
      expect(store.entityMap()[R2]?.title).toBe('Renamed');
      expect(resultIds()).toEqual([R2, R3]);
      expect(store.pageNumber()).toBe(2);
    });

    it('stays fulfilled for an empty result', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([], { totalPages: 0 }));

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.results()).toEqual([]);
    });

    it('cancels a pending search when a newer one starts', () => {
      store.search(BREWER_A, { pageNumber: 1 }, {});
      const first = expectSearch();
      store.search(BREWER_A, { pageNumber: 2 }, {});
      const second = expectSearch();

      expect(first.cancelled).toBe(true);
      second.flush(makePage([makeReview(R3)], { pageNumber: 2 }));
      expect(resultIds()).toEqual([R3]);
    });

    it('sets a server ApiError on 500 and recovers on the next search', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush('boom', serverError);
      expect(store.error()?.kind).toBe('server');

      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)]));
      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(idsOf()).toEqual([R1]);
    });

    it('sets a network ApiError on status 0', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().error(new ProgressEvent('error'));
      expect(store.error()?.kind).toBe('network');
    });

    it('records an invalid rating filter as an error without a request', () => {
      store.search(BREWER_A, {}, { minimumRating: 9 });

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('getById', () => {
    it('upserts and exposes the entity', () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);

      store.getById(BREWER_A, R2);
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R2}`).flush(makeReview(R2));

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([R1, R2]);
    });

    it('records a 404 as a client ApiError and leaves entities alone', () => {
      store.getById(BREWER_A, R1);
      httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R1}`).flush(null, notFound);

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.status).toBe(404);
      expect(store.entities()).toEqual([]);
    });

    it('records an invalid review id as an error without a request', () => {
      store.getById(BREWER_A, 'not-a-guid');

      expect(store.error()?.messages).toEqual([
        'Review id must be a valid GUID.',
      ]);
      httpMock.expectNone(() => true);
    });
  });

  describe('switching brewers', () => {
    it('empties the store and activates the new brewer', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)], { totalRecords: 7 }));
      expect(idsOf()).toEqual([R1]);

      store.list(BREWER_B);

      expect(store.activeBrewerId()).toBe(BREWER_B);
      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.pageNumber()).toBe(1);
      expect(store.isPending()).toBe(true);
      httpMock
        .expectOne(reviewsUrl(BREWER_B))
        .flush([makeReview(R2, BREWER_B)]);
      expect(idsOf()).toEqual([R2]);
    });

    it('clears a previous error', () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush('boom', serverError);
      expect(store.error()).not.toBeNull();

      store.list(BREWER_B);

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(reviewsUrl(BREWER_B)).flush([]);
    });

    it('does not clear anything when the same brewer is used again', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)]));

      store.list(BREWER_A);

      expect(idsOf()).toEqual([R1]);
      expect(resultIds()).toEqual([R1]);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
    });

    it('ignores a late response for the previous brewer and keeps the status untouched', () => {
      store.list(BREWER_A);
      const late = httpMock.expectOne(reviewsUrl(BREWER_A));
      store.search(BREWER_B, {}, {});
      const current = expectSearch(BREWER_B);

      late.flush([makeReview(R1)]);

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
      expect(store.activeBrewerId()).toBe(BREWER_B);

      current.flush(makePage([makeReview(R2, BREWER_B)]));
      expect(idsOf()).toEqual([R2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a late error for the previous brewer', () => {
      store.list(BREWER_A);
      const late = httpMock.expectOne(reviewsUrl(BREWER_A));
      store.search(BREWER_B, {}, {});
      const current = expectSearch(BREWER_B);

      late.flush('boom', serverError);

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      current.flush(makePage([]));
      expect(store.isFulfilled()).toBe(true);
    });

    it('isolates every kind of read from a brewer switch', async () => {
      store.list(BREWER_A);
      const lateList = httpMock.expectOne(reviewsUrl(BREWER_A));
      store.search(BREWER_A, {}, {});
      const lateSearch = expectSearch(BREWER_A);
      store.getById(BREWER_A, R3);
      const lateOne = httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R3}`);

      // A write is the one call that switches brewers without cancelling any of the reads.
      const switching = store.create(BREWER_B, writePayload);
      const current = httpMock.expectOne(reviewsUrl(BREWER_B));

      lateList.flush([makeReview(R1)]);
      lateSearch.flush(makePage([makeReview(R2)], { totalRecords: 9 }));
      lateOne.flush(makeReview(R3));

      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.isPending()).toBe(true);

      current.flush(makeReview(R1, BREWER_B), {
        status: 201,
        statusText: 'Created',
      });
      await switching;
      expect(idsOf()).toEqual([R1]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('returns to an earlier brewer with an empty store', () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
      store.list(BREWER_B);
      httpMock
        .expectOne(reviewsUrl(BREWER_B))
        .flush([makeReview(R2, BREWER_B)]);

      store.list(BREWER_A);

      expect(store.activeBrewerId()).toBe(BREWER_A);
      expect(store.entities()).toEqual([]);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([]);
    });

    it('ignores a write response for the previous brewer', async () => {
      const pending = store.create(BREWER_A, writePayload);
      const late = httpMock.expectOne(reviewsUrl(BREWER_A));

      store.list(BREWER_B);
      const current = httpMock.expectOne(reviewsUrl(BREWER_B));

      late.flush(makeReview(R1), { status: 201, statusText: 'Created' });
      await pending;

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);

      current.flush([]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a write error for the previous brewer', async () => {
      const pending = store.delete(BREWER_A, R1);
      const late = httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R1}`);

      store.list(BREWER_B);
      const current = httpMock.expectOne(reviewsUrl(BREWER_B));

      late.flush('boom', serverError);
      await pending;

      expect(store.error()).toBeNull();
      current.flush([]);
      expect(store.isFulfilled()).toBe(true);
    });
  });

  describe('create', () => {
    it('adds the created review as an entity and does not touch results or totals', async () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)]));

      const pending = store.create(BREWER_A, writePayload);
      expect(store.isPending()).toBe(true);
      const request = httpMock.expectOne(reviewsUrl(BREWER_A));
      expect(request.request.method).toBe('POST');
      request.flush(makeReview(R2), { status: 201, statusText: 'Created' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([R1, R2]);
      expect(resultIds()).toEqual([R1]);
      expect(store.totalRecords()).toBe(1);
    });

    it('resolves and exposes 400 messages from a string array', async () => {
      const pending = store.create(BREWER_A, writePayload);
      httpMock
        .expectOne(reviewsUrl(BREWER_A))
        .flush(['Title is required', 'Rating is out of range'], {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual([
        'Title is required',
        'Rating is out of range',
      ]);
      expect(store.entities()).toEqual([]);
    });

    it('leaves the store in an error state without a request for invalid input', async () => {
      await expect(
        store.create(BREWER_A, { ...writePayload, rating: 0 }),
      ).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('delete', () => {
    it('removes the entity from entities and results without adjusting totals', async () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1), makeReview(R2)]));

      const pending = store.delete(BREWER_A, R1);
      const request = httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R1}`);
      expect(request.request.method).toBe('DELETE');
      request.flush(null, { status: 204, statusText: 'No Content' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([R2]);
      expect(resultIds()).toEqual([R2]);
      expect(store.totalRecords()).toBe(2);
    });

    it('keeps the entity and exposes the message on a 400', async () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)]));

      const pending = store.delete(BREWER_A, R1);
      httpMock
        .expectOne(`${reviewsUrl(BREWER_A)}/${R1}`)
        .flush('Review is not deletable', {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual(['Review is not deletable']);
      expect(idsOf()).toEqual([R1]);
      expect(resultIds()).toEqual([R1]);
    });

    it('keeps the entity on a 404', async () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);

      const pending = store.delete(BREWER_A, R1);
      httpMock
        .expectOne(`${reviewsUrl(BREWER_A)}/${R1}`)
        .flush('Not found', notFound);
      await pending;

      expect(store.error()?.status).toBe(404);
      expect(idsOf()).toEqual([R1]);
    });

    it('errors without a request for an invalid review id', async () => {
      await expect(
        store.delete(BREWER_A, 'not-a-guid'),
      ).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('reset', () => {
    it('clears the active brewer, entities, page and status', () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(makePage([makeReview(R1)], { totalRecords: 4 }));

      store.reset();

      expect(store.activeBrewerId()).toBeNull();
      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.isIdle()).toBe(true);
    });

    it('ignores a response that arrives after the reset', () => {
      store.list(BREWER_A);
      const late = httpMock.expectOne(reviewsUrl(BREWER_A));

      store.reset();
      late.flush([makeReview(R1)]);

      expect(store.entities()).toEqual([]);
      expect(store.isIdle()).toBe(true);
    });
  });

  describe('stale responses after the store was cleared', () => {
    it.each([
      {
        method: 'list',
        returnToA: () => store.list(BREWER_A),
        url: reviewsUrl(BREWER_A),
        body: [makeReview(R2)],
        expectedId: R2,
      },
      {
        method: 'getById',
        returnToA: () => store.getById(BREWER_A, R3),
        url: `${reviewsUrl(BREWER_A)}/${R3}`,
        body: makeReview(R3),
        expectedId: R3,
      },
    ])(
      'ignores an older search(A) response after A, B, A through $method',
      ({ returnToA, url, body, expectedId }) => {
        store.search(BREWER_A, {}, {});
        const stale = expectSearch(BREWER_A);
        store.list(BREWER_B);
        httpMock.expectOne(reviewsUrl(BREWER_B));
        returnToA();
        const current = httpMock.expectOne(url);

        stale.flush(makePage([makeReview(R1)], { totalRecords: 9 }));

        expect(store.entities()).toEqual([]);
        expect(store.results()).toEqual([]);
        expect(store.totalRecords()).toBe(0);
        expect(store.isPending()).toBe(true);

        current.flush(body);
        expect(store.isFulfilled()).toBe(true);
        expect(resultIds()).toEqual([]);
        expect(idsOf()).toEqual([expectedId]);
      },
    );

    it('ignores an older list(A) error after A, B, A and leaves the status pending', () => {
      store.list(BREWER_A);
      const stale = httpMock.expectOne(reviewsUrl(BREWER_A));
      store.search(BREWER_B, {}, {});
      expectSearch(BREWER_B);
      store.getById(BREWER_A, R3);
      const current = httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R3}`);

      stale.flush('boom', serverError);

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      current.flush(makeReview(R3));
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores an older response after reset() followed by the same brewer', () => {
      store.search(BREWER_A, {}, {});
      const stale = expectSearch(BREWER_A);
      store.reset();
      store.list(BREWER_A);
      const current = httpMock.expectOne(reviewsUrl(BREWER_A));

      stale.flush(makePage([makeReview(R1)], { totalRecords: 9 }));

      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.isPending()).toBe(true);
      current.flush([makeReview(R2)]);
      expect(idsOf()).toEqual([R2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a create(A) response that resolves after A, B, A', async () => {
      const pending = store.create(BREWER_A, writePayload);
      const stale = httpMock.expectOne(reviewsUrl(BREWER_A));
      store.list(BREWER_B);
      httpMock.expectOne(reviewsUrl(BREWER_B));
      store.list(BREWER_A);
      const current = httpMock.expectOne(reviewsUrl(BREWER_A));

      stale.flush(makeReview(R1), { status: 201, statusText: 'Created' });
      await pending;

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
      current.flush([makeReview(R2)]);
      expect(idsOf()).toEqual([R2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a delete(A) response that resolves after A, B, A', async () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
      const pending = store.delete(BREWER_A, R1);
      const stale = httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R1}`);
      store.list(BREWER_B);
      httpMock.expectOne(reviewsUrl(BREWER_B));
      store.search(BREWER_A, {}, {});
      expectSearch(BREWER_A).flush(makePage([makeReview(R1), makeReview(R2)]));
      expect(store.isFulfilled()).toBe(true);

      stale.flush(null, { status: 204, statusText: 'No Content' });
      await pending;

      expect(idsOf()).toEqual([R1, R2]);
      expect(resultIds()).toEqual([R1, R2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a delete(A) error that resolves after A, B, A', async () => {
      const pending = store.delete(BREWER_A, R1);
      const stale = httpMock.expectOne(`${reviewsUrl(BREWER_A)}/${R1}`);
      store.list(BREWER_B);
      httpMock.expectOne(reviewsUrl(BREWER_B));
      store.list(BREWER_A);
      const current = httpMock.expectOne(reviewsUrl(BREWER_A));

      stale.flush('boom', serverError);
      await pending;

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      current.flush([]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a write response that resolves after reset() and the same brewer again', async () => {
      const pending = store.create(BREWER_A, writePayload);
      const stale = httpMock.expectOne(reviewsUrl(BREWER_A));
      store.reset();
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A));

      stale.flush(makeReview(R1), { status: 201, statusText: 'Created' });
      await pending;

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
    });
  });

  describe('ids are case-insensitive', () => {
    const upperBrewer = BREWER_A.toUpperCase();
    const upperReview = REVIEW_WITH_LETTERS.toUpperCase();

    it('does not treat a different letter case of the active brewer as a switch', () => {
      store.list(upperBrewer);
      httpMock.expectOne(reviewsUrl(upperBrewer)).flush([makeReview(R1)]);
      expect(store.activeBrewerId()).toBe(BREWER_A);

      store.list(BREWER_A);

      expect(idsOf()).toEqual([R1]);
      expect(store.activeBrewerId()).toBe(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
    });

    it('applies a response for the same brewer in another letter case', () => {
      store.list(upperBrewer);
      const first = httpMock.expectOne(reviewsUrl(upperBrewer));
      store.search(BREWER_A, {}, {});
      expectSearch(BREWER_A);

      first.flush([makeReview(R1)]);

      expect(idsOf()).toEqual([R1]);
    });

    it('removes the entity and the results row when deleted by an uppercase id', async () => {
      store.search(BREWER_A, {}, {});
      expectSearch().flush(
        makePage([makeReview(REVIEW_WITH_LETTERS), makeReview(R2)]),
      );

      const pending = store.delete(upperBrewer, upperReview);
      httpMock
        .expectOne(`${reviewsUrl(upperBrewer)}/${upperReview}`)
        .flush(null, { status: 204, statusText: 'No Content' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([R2]);
      expect(resultIds()).toEqual([R2]);
    });

    it('keys entities by the lowercase review id even when the server sends uppercase', () => {
      store.list(BREWER_A);
      httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(upperReview)]);

      expect(idsOf()).toEqual([REVIEW_WITH_LETTERS]);
    });
  });

  describe('malformed runtime input', () => {
    const asString = (value: unknown) => value as string;

    it.each([null, undefined, 42])(
      'list(%j) records a client error and the store keeps working',
      (brewerId) => {
        store.list(asString(brewerId));

        expect(store.error()?.kind).toBe('client');
        expect(store.error()?.status).toBe(400);
        httpMock.expectNone(() => true);

        store.list(BREWER_A);
        httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([R1]);
      },
    );

    it.each([null, undefined, 42])(
      'getById with the review id %j records a client error and the store keeps working',
      (reviewId) => {
        store.getById(BREWER_A, asString(reviewId));

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);

        store.getById(BREWER_A, R1);
        httpMock
          .expectOne(`${reviewsUrl(BREWER_A)}/${R1}`)
          .flush(makeReview(R1));
        expect(store.isFulfilled()).toBe(true);
      },
    );

    it.each([null, undefined, 'filters'])(
      'search with the filters %j records a client error and the store keeps working',
      (filters) => {
        store.search(
          BREWER_A,
          {},
          filters as unknown as BrewerReviewSearchFilters,
        );

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);

        store.search(BREWER_A, {}, {});
        expectSearch().flush(makePage([makeReview(R1)]));
        expect(store.isFulfilled()).toBe(true);
        expect(resultIds()).toEqual([R1]);
      },
    );

    it('search treats a missing page as no page params and keeps working', () => {
      store.search(BREWER_A, undefined as unknown as BrewerReviewPageQuery, {});
      const request = expectSearch();
      expect(request.request.params.keys()).toEqual([]);
      request.flush(makePage([makeReview(R1)]));
      expect(store.isFulfilled()).toBe(true);
    });

    it('search records a client error for a non-GUID id filter', () => {
      store.search(BREWER_A, {}, { id: 'x' });

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });

    it.each([null, undefined, 'review'])(
      'create with the review %j resolves with a client error and the store keeps working',
      async (review) => {
        await expect(
          store.create(BREWER_A, review as unknown as CreateBrewerReview),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        expect(store.error()?.status).toBe(400);
        httpMock.expectNone(() => true);

        const pending = store.create(BREWER_A, writePayload);
        httpMock
          .expectOne(reviewsUrl(BREWER_A))
          .flush(makeReview(R1), { status: 201, statusText: 'Created' });
        await pending;
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([R1]);
      },
    );

    it.each([null, undefined, 42])(
      'create with the brewer id %j resolves with a client error',
      async (brewerId) => {
        await expect(
          store.create(asString(brewerId), writePayload),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);
      },
    );

    it.each([null, undefined, 42])(
      'delete with the review id %j resolves with a client error and the store keeps working',
      async (reviewId) => {
        await expect(
          store.delete(BREWER_A, asString(reviewId)),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);

        const pending = store.delete(BREWER_A, R1);
        httpMock
          .expectOne(`${reviewsUrl(BREWER_A)}/${R1}`)
          .flush(null, { status: 204, statusText: 'No Content' });
        await pending;
        expect(store.isFulfilled()).toBe(true);
      },
    );
  });

  describe('an API call that throws synchronously', () => {
    const boom = () => {
      throw new TypeError('boom');
    };

    it.each(['list', 'search', 'getById'] as const)(
      '%s lands in error() and the rxMethod keeps working',
      (method) => {
        const spy = vi
          .spyOn(TestBed.inject(BrewerReviewApiService), method)
          .mockImplementationOnce(boom);

        if (method === 'list') store.list(BREWER_A);
        if (method === 'search') store.search(BREWER_A, {}, {});
        if (method === 'getById') store.getById(BREWER_A, R1);

        expect(store.error()).toBeInstanceOf(ApiError);
        expect(spy).toHaveBeenCalledTimes(1);

        store.list(BREWER_A);
        httpMock.expectOne(reviewsUrl(BREWER_A)).flush([makeReview(R1)]);
        expect(store.isFulfilled()).toBe(true);
      },
    );

    it.each(['create', 'delete'] as const)(
      '%s resolves with error() set instead of throwing',
      async (method) => {
        vi.spyOn(
          TestBed.inject(BrewerReviewApiService),
          method,
        ).mockImplementationOnce(boom);

        await expect(
          method === 'create'
            ? store.create(BREWER_A, writePayload)
            : store.delete(BREWER_A, R1),
        ).resolves.toBeUndefined();

        expect(store.error()).toBeInstanceOf(ApiError);
      },
    );
  });

  describe('no update', () => {
    it('exposes no update or patch method, because the API has no review update endpoint', () => {
      expect('update' in store).toBe(false);
      expect('patch' in store).toBe(false);
      expect('updateReview' in store).toBe(false);
    });

    it('rejects an update call at compile time', () => {
      // @ts-expect-error the store has no update method
      expect(() => store.update(BREWER_A, R1, writePayload)).toThrow();
    });
  });
});
