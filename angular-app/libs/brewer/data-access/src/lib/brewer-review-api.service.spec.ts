import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import { firstValueFrom, type Observable } from 'rxjs';
import { BrewerReviewApiService } from './brewer-review-api.service';
import type {
  BrewerReview,
  BrewerReviewPageQuery,
  BrewerReviewSearchFilters,
  CreateBrewerReview,
} from './brewer-review.models';

const BASE_URL = 'https://api.test/api/v1';
const BREWER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const REVIEW_ID = '9b2c4a10-1f5e-4c1d-8a3b-7d6e5f4a3b21';
const REVIEWS_URL = `${BASE_URL}/brewers/${BREWER_ID}/reviews`;
const SEARCH_URL = `${REVIEWS_URL}/search`;

const createReview: CreateBrewerReview = {
  reviewerName: 'Sam',
  title: 'Great',
  comments: 'Would visit again',
  rating: 4,
};

function makeReview(): BrewerReview {
  return {
    ...createReview,
    reviewId: REVIEW_ID,
    brewerId: BREWER_ID,
    isDeletable: true,
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

describe('BrewerReviewApiService', () => {
  let httpMock: HttpTestingController;
  let api: BrewerReviewApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    api = TestBed.inject(BrewerReviewApiService);
  });

  afterEach(() => httpMock.verify());

  async function expectClientError(
    result: Promise<unknown>,
    messageFragment: string,
  ): Promise<void> {
    const error: unknown = await result.catch((caught: unknown) => caught);
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).kind).toBe('client');
    expect((error as ApiError).messages.join(' ')).toContain(messageFragment);
  }

  function searchRequest(
    page: Parameters<BrewerReviewApiService['search']>[1],
    filters: Parameters<BrewerReviewApiService['search']>[2] = {},
  ) {
    api.search(BREWER_ID, page, filters).subscribe();
    const request = httpMock.expectOne((r) => r.url === SEARCH_URL);
    request.flush({});
    return request.request;
  }

  describe('list', () => {
    it('GETs /brewers/{brewerId}/reviews', async () => {
      const result = firstValueFrom(api.list(BREWER_ID));
      const request = httpMock.expectOne(REVIEWS_URL);
      expect(request.request.method).toBe('GET');
      request.flush([makeReview()]);
      expect(await result).toHaveLength(1);
    });

    it('maps a 404 to an empty list', async () => {
      const result = firstValueFrom(api.list(BREWER_ID));
      httpMock
        .expectOne(REVIEWS_URL)
        .flush('No reviews', { status: 404, statusText: 'Not Found' });
      expect(await result).toEqual([]);
    });

    it('propagates other errors', async () => {
      const result = firstValueFrom(api.list(BREWER_ID));
      httpMock
        .expectOne(REVIEWS_URL)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      await expect(result).rejects.toMatchObject({ status: 500 });
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(firstValueFrom(api.list('a/b c')), 'valid GUID');
      httpMock.expectNone(() => true);
    });
  });

  describe('getById', () => {
    it('GETs /brewers/{brewerId}/reviews/{reviewId}', () => {
      api.getById(BREWER_ID, REVIEW_ID).subscribe();
      const request = httpMock.expectOne(`${REVIEWS_URL}/${REVIEW_ID}`);
      expect(request.request.method).toBe('GET');
      request.flush(makeReview());
    });

    it('propagates a 404', async () => {
      const result = firstValueFrom(api.getById(BREWER_ID, REVIEW_ID));
      httpMock
        .expectOne(`${REVIEWS_URL}/${REVIEW_ID}`)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(result).rejects.toMatchObject({ status: 404 });
    });

    it.each([
      ['brewer', 'a/b c', REVIEW_ID],
      ['review', BREWER_ID, 'a/b c'],
    ])(
      'sends no request for a %s id that is not a GUID',
      async (_, brewerId, reviewId) => {
        await expectClientError(
          firstValueFrom(api.getById(brewerId, reviewId)),
          'valid GUID',
        );
        httpMock.expectNone(() => true);
      },
    );
  });

  describe('search', () => {
    it('POSTs to /search with an empty body and no query string by default', () => {
      api.search(BREWER_ID, {}, {}).subscribe();
      const request = httpMock.expectOne(SEARCH_URL);
      expect(request.request.method).toBe('POST');
      expect(request.request.params.keys()).toEqual([]);
      expect(request.request.body).toEqual({});
      request.flush({});
    });

    it('sends all page params in PascalCase', () => {
      const request = searchRequest({
        pageNumber: 3,
        pageSize: 20,
        sortOrder: 'desc',
        orderBy: 'rating',
      });
      expect(request.params.get('PageNumber')).toBe('3');
      expect(request.params.get('PageSize')).toBe('20');
      expect(request.params.get('SortOrder')).toBe('desc');
      expect(request.params.get('OrderBy')).toBe('rating');
    });

    it('sends only the page params that are provided', () => {
      const request = searchRequest({ orderBy: 'reviewerName' });
      expect(request.params.keys()).toEqual(['OrderBy']);
    });

    it.each([
      [0, '1'],
      [-5, '1'],
      [1, '1'],
      [50, '50'],
      [51, '50'],
      [100, '50'],
      [2.7, '2'],
      [Number.POSITIVE_INFINITY, null],
      [Number.NaN, null],
    ])('clamps PageSize %d to %s', (pageSize, expected) => {
      expect(searchRequest({ pageSize }).params.get('PageSize')).toBe(expected);
    });

    it.each([
      [0, '1'],
      [-3, '1'],
      [1, '1'],
      [4.9, '4'],
      [Number.NaN, null],
    ])('floors PageNumber %d at %s', (pageNumber, expected) => {
      expect(searchRequest({ pageNumber }).params.get('PageNumber')).toBe(
        expected,
      );
    });

    it('includes only the filters that are set', () => {
      const request = searchRequest(
        {},
        { name: 'ipa', minimumRating: 2, maximumRating: 5 },
      );
      expect(request.body).toEqual({
        name: 'ipa',
        minimumRating: 2,
        maximumRating: 5,
      });
    });

    it('drops blank text filters', () => {
      const request = searchRequest(
        {},
        { id: '', name: '   ', reviewerName: 'Sam' },
      );
      expect(request.body).toEqual({ reviewerName: 'Sam' });
    });

    it('never sends brewerId in the body, even when cast in', () => {
      const filters = { name: 'ipa', brewerId: BREWER_ID } as object;
      const request = searchRequest({}, filters);
      expect(request.body).toEqual({ name: 'ipa' });
      expect(request.url).toContain(BREWER_ID);
    });

    it('sends no request for a rating outside 1 to 5', async () => {
      await expectClientError(
        firstValueFrom(api.search(BREWER_ID, {}, { minimumRating: 0 })),
        'Minimum rating',
      );
      await expectClientError(
        firstValueFrom(api.search(BREWER_ID, {}, { maximumRating: 6 })),
        'Maximum rating',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request when the minimum exceeds the maximum', async () => {
      await expectClientError(
        firstValueFrom(
          api.search(BREWER_ID, {}, { minimumRating: 5, maximumRating: 1 }),
        ),
        'must not be greater',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.search('a/b c', {}, {})),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('create', () => {
    it('POSTs exactly the four review fields', () => {
      api.create(BREWER_ID, { ...makeReview() }).subscribe();
      const request = httpMock.expectOne(REVIEWS_URL);
      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(createReview);
      expect(Object.keys(request.request.body as object).sort()).toEqual([
        'comments',
        'rating',
        'reviewerName',
        'title',
      ]);
      request.flush(makeReview(), { status: 201, statusText: 'Created' });
    });

    it('sends no request for invalid input', async () => {
      await expectClientError(
        firstValueFrom(api.create(BREWER_ID, { ...createReview, rating: 6 })),
        'Rating',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.create('a/b c', createReview)),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('delete', () => {
    it('DELETEs /brewers/{brewerId}/reviews/{reviewId}', () => {
      api.delete(BREWER_ID, REVIEW_ID).subscribe();
      const request = httpMock.expectOne(`${REVIEWS_URL}/${REVIEW_ID}`);
      expect(request.request.method).toBe('DELETE');
      request.flush(null, { status: 204, statusText: 'No Content' });
    });

    it('sends no request for a review id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.delete(BREWER_ID, 'a/b c')),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('malformed runtime input', () => {
    async function expectClientRejection(
      request: () => Observable<unknown>,
    ): Promise<void> {
      const error: unknown = await firstValueFrom(request()).catch(
        (caught: unknown) => caught,
      );
      expect(error).toBeInstanceOf(ApiError);
      expect((error as ApiError).kind).toBe('client');
      expect((error as ApiError).status).toBe(400);
      httpMock.expectNone(() => true);
    }

    const bad = [null, undefined, 42, {}] as unknown as string[];

    it.each(bad)('list rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.list(id));
    });

    it.each(bad)('getById rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.getById(id, REVIEW_ID));
    });

    it.each(bad)('getById rejects the review id %j', async (id) => {
      await expectClientRejection(() => api.getById(BREWER_ID, id));
    });

    it.each(bad)('delete rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.delete(id, REVIEW_ID));
    });

    it.each(bad)('delete rejects the review id %j', async (id) => {
      await expectClientRejection(() => api.delete(BREWER_ID, id));
    });

    it.each(bad)('create rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.create(id, createReview));
    });

    it.each([null, undefined, 'review'])(
      'create rejects the review %j',
      async (review) => {
        await expectClientRejection(() =>
          api.create(BREWER_ID, review as unknown as CreateBrewerReview),
        );
      },
    );

    it.each(bad)('search rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.search(id, {}, {}));
    });

    it.each([null, undefined, 'filters'])(
      'search rejects the filters %j',
      async (filters) => {
        await expectClientRejection(() =>
          api.search(
            BREWER_ID,
            {},
            filters as unknown as BrewerReviewSearchFilters,
          ),
        );
      },
    );

    it.each([null, undefined])(
      'search sends no page params for the page %j',
      (page) => {
        const request = searchRequest(page as unknown as BrewerReviewPageQuery);
        expect(request.params.keys()).toEqual([]);
      },
    );

    it('search rejects an id filter that is not a GUID', async () => {
      await expectClientRejection(() =>
        api.search(BREWER_ID, {}, { id: 'not-a-guid' }),
      );
    });

    it('search sends a GUID id filter', () => {
      expect(searchRequest({}, { id: REVIEW_ID }).body).toEqual({
        id: REVIEW_ID,
      });
    });

    it('search drops text filters that are not strings instead of throwing', () => {
      const filters = {
        name: 7,
        reviewerName: {},
      } as unknown as BrewerReviewSearchFilters;
      expect(searchRequest({}, filters).body).toEqual({});
    });
  });

  it('has no update or patch method, because the API has no review update endpoint', () => {
    expect('update' in api).toBe(false);
    expect('patch' in api).toBe(false);
  });
});
