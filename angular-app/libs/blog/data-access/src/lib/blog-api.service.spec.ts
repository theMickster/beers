import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import { firstValueFrom, type Observable } from 'rxjs';
import { BlogApiService } from './blog-api.service';
import type {
  BlogPostPageQuery,
  BlogPostSearchFilters,
  CreateNewsBlogPost,
  NewsBlogPost,
} from './blog.models';

const BASE_URL = 'https://api.test/api/v1';
const BREWER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const OTHER_BREWER_ID = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
const POST_ID = '9b2c4a10-1f5e-4c1d-8a3b-7d6e5f4a3b21';
const OTHER_POST_ID = '1b4e28ba-2fa1-11d2-883f-0016d3cca427';
const POSTS_URL = `${BASE_URL}/brewers/${BREWER_ID}/posts`;
const POST_URL = `${POSTS_URL}/${POST_ID}`;
const SEARCH_URL = `${BASE_URL}/posts/search`;

const createPost: CreateNewsBlogPost = {
  title: 'Grand opening',
  body: 'Doors open at noon.',
  postType: 'TextPost',
  tags: ['news'],
  imageUrls: [],
  eventDate: null,
  eventLocation: null,
  publishedDate: '2026-01-01T00:00:00Z',
};

function makePost(): NewsBlogPost {
  return {
    ...createPost,
    newsBlogPostId: POST_ID,
    brewerId: BREWER_ID,
    author: {
      id: BREWER_ID,
      name: 'Acme Brewing',
      website: 'https://acme.test',
    },
    isDeletable: true,
  };
}

describe('BlogApiService', () => {
  let httpMock: HttpTestingController;
  let api: BlogApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    api = TestBed.inject(BlogApiService);
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
    page: BlogPostPageQuery,
    filters: BlogPostSearchFilters = {},
  ) {
    api.search(page, filters).subscribe();
    const request = httpMock.expectOne((r) => r.url === SEARCH_URL);
    request.flush({});
    return request.request;
  }

  describe('listByBrewer', () => {
    it('GETs /brewers/{brewerId}/posts', async () => {
      const result = firstValueFrom(api.listByBrewer(BREWER_ID));
      const request = httpMock.expectOne(POSTS_URL);
      expect(request.request.method).toBe('GET');
      request.flush([makePost()]);
      expect(await result).toHaveLength(1);
    });

    it('returns an empty list for an unknown brewer', async () => {
      const result = firstValueFrom(api.listByBrewer(OTHER_BREWER_ID));
      httpMock
        .expectOne(`${BASE_URL}/brewers/${OTHER_BREWER_ID}/posts`)
        .flush([]);
      expect(await result).toEqual([]);
    });

    it('propagates a 404 instead of mapping it to an empty list', async () => {
      const result = firstValueFrom(api.listByBrewer(BREWER_ID));
      httpMock
        .expectOne(POSTS_URL)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(result).rejects.toMatchObject({ status: 404 });
    });

    it('propagates a 500', async () => {
      const result = firstValueFrom(api.listByBrewer(BREWER_ID));
      httpMock
        .expectOne(POSTS_URL)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      await expect(result).rejects.toMatchObject({ status: 500 });
    });

    it('propagates a network error', async () => {
      const result = firstValueFrom(api.listByBrewer(BREWER_ID));
      httpMock.expectOne(POSTS_URL).error(new ProgressEvent('error'));
      await expect(result).rejects.toMatchObject({ status: 0 });
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.listByBrewer('a/b c')),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('getById', () => {
    it('GETs /brewers/{brewerId}/posts/{postId}', () => {
      api.getById(BREWER_ID, POST_ID).subscribe();
      const request = httpMock.expectOne(POST_URL);
      expect(request.request.method).toBe('GET');
      request.flush(makePost());
    });

    it('propagates a 404', async () => {
      const result = firstValueFrom(api.getById(BREWER_ID, POST_ID));
      httpMock
        .expectOne(POST_URL)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(result).rejects.toMatchObject({ status: 404 });
    });

    it.each([
      ['brewer', 'a/b c', POST_ID],
      ['post', BREWER_ID, 'a/b c'],
    ])(
      'sends no request for a %s id that is not a GUID',
      async (_, brewerId, postId) => {
        await expectClientError(
          firstValueFrom(api.getById(brewerId, postId)),
          'valid GUID',
        );
        httpMock.expectNone(() => true);
      },
    );
  });

  describe('search', () => {
    it('POSTs to /posts/search with an empty body and no query string by default', () => {
      api.search({}, {}).subscribe();
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
        orderBy: 'publishedDate',
      });
      expect(request.params.get('PageNumber')).toBe('3');
      expect(request.params.get('PageSize')).toBe('20');
      expect(request.params.get('SortOrder')).toBe('desc');
      expect(request.params.get('OrderBy')).toBe('publishedDate');
    });

    it('sends only the page params that are provided', () => {
      const request = searchRequest({ orderBy: 'title' });
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

    it('sends every filter in the body', () => {
      const filters: BlogPostSearchFilters = {
        brewerId: BREWER_ID,
        postType: 'EventAnnouncement',
        tag: 'taproom',
        dateRangeStart: '2026-01-01',
        dateRangeEnd: '2026-02-01T00:00:00Z',
        includeDrafts: true,
      };
      expect(searchRequest({}, filters).body).toEqual(filters);
    });

    it('sends includeDrafts when it is false', () => {
      expect(searchRequest({}, { includeDrafts: false }).body).toEqual({
        includeDrafts: false,
      });
    });

    it('sends the tag as one string, not a list', () => {
      const { body } = searchRequest({}, { tag: 'ipa' });
      expect(body).toEqual({ tag: 'ipa' });
      expect(body).not.toHaveProperty('tags');
    });

    it('drops blank text filters', () => {
      const request = searchRequest(
        {},
        { brewerId: '', tag: '   ', postType: undefined, dateRangeStart: '' },
      );
      expect(request.body).toEqual({});
    });

    it('sends no request for a brewer id filter that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.search({}, { brewerId: 'a/b c' })),
        'Brewer id filter',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for an unknown post type', async () => {
      const filters = {
        postType: 'Podcast',
      } as unknown as BlogPostSearchFilters;
      await expectClientError(
        firstValueFrom(api.search({}, filters)),
        'Post type must be a valid value',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for a date that is not ISO 8601', async () => {
      await expectClientError(
        firstValueFrom(api.search({}, { dateRangeStart: 'Jan 1 2026' })),
        'Date range start',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request when the end is before the start', async () => {
      await expectClientError(
        firstValueFrom(
          api.search(
            {},
            { dateRangeStart: '2026-02-01', dateRangeEnd: '2026-01-01' },
          ),
        ),
        'DateRangeEnd must be greater than or equal to DateRangeStart',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('create', () => {
    it('POSTs exactly the post fields and no ids', () => {
      api
        .create(BREWER_ID, {
          ...createPost,
          extra: 'leak',
        } as CreateNewsBlogPost)
        .subscribe();
      const request = httpMock.expectOne(POSTS_URL);
      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(createPost);
      expect(Object.keys(request.request.body as object).sort()).toEqual([
        'body',
        'eventDate',
        'eventLocation',
        'imageUrls',
        'postType',
        'publishedDate',
        'tags',
        'title',
      ]);
      request.flush(makePost(), { status: 201, statusText: 'Created' });
    });

    it('sends no request for invalid input', async () => {
      await expectClientError(
        firstValueFrom(api.create(BREWER_ID, { ...createPost, title: ' ' })),
        'Title cannot be null, empty, or whitespace',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.create('a/b c', createPost)),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('update', () => {
    it('PUTs the post with both ids taken from the arguments', () => {
      api
        .update(BREWER_ID, POST_ID, {
          ...createPost,
          brewerId: OTHER_BREWER_ID,
          newsBlogPostId: OTHER_POST_ID,
        } as CreateNewsBlogPost)
        .subscribe();
      const request = httpMock.expectOne(POST_URL);
      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual({
        ...createPost,
        brewerId: BREWER_ID,
        newsBlogPostId: POST_ID,
      });
      request.flush(makePost());
    });

    it('sends no request for invalid input', async () => {
      await expectClientError(
        firstValueFrom(
          api.update(BREWER_ID, POST_ID, { ...createPost, body: '' }),
        ),
        'Body cannot be null, empty, or whitespace',
      );
      httpMock.expectNone(() => true);
    });

    it.each([
      ['brewer', 'a/b c', POST_ID],
      ['post', BREWER_ID, 'a/b c'],
    ])(
      'sends no request for a %s id that is not a GUID',
      async (_, brewerId, postId) => {
        await expectClientError(
          firstValueFrom(api.update(brewerId, postId, createPost)),
          'valid GUID',
        );
        httpMock.expectNone(() => true);
      },
    );

    it('propagates a 400 from the server', async () => {
      const result = firstValueFrom(api.update(BREWER_ID, POST_ID, createPost));
      httpMock.expectOne(POST_URL).flush(['NewsBlogPost does not exist'], {
        status: 400,
        statusText: 'Bad Request',
      });
      await expect(result).rejects.toMatchObject({ status: 400 });
    });
  });

  describe('delete', () => {
    it('DELETEs /brewers/{brewerId}/posts/{postId}', () => {
      api.delete(BREWER_ID, POST_ID).subscribe();
      const request = httpMock.expectOne(POST_URL);
      expect(request.request.method).toBe('DELETE');
      request.flush(null, { status: 204, statusText: 'No Content' });
    });

    it('sends no request for a post id that is not a GUID', async () => {
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

    it.each(bad)('listByBrewer rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.listByBrewer(id));
    });

    it.each(bad)('getById rejects the brewer id %j', async (id) => {
      await expectClientRejection(() => api.getById(id, POST_ID));
    });

    it.each(bad)('getById rejects the post id %j', async (id) => {
      await expectClientRejection(() => api.getById(BREWER_ID, id));
    });

    it.each(bad)('delete rejects the post id %j', async (id) => {
      await expectClientRejection(() => api.delete(BREWER_ID, id));
    });

    it.each(bad)('update rejects the post id %j', async (id) => {
      await expectClientRejection(() => api.update(BREWER_ID, id, createPost));
    });

    it.each([null, undefined, 'post'])(
      'create rejects the post %j',
      async (post) => {
        await expectClientRejection(() =>
          api.create(BREWER_ID, post as unknown as CreateNewsBlogPost),
        );
      },
    );

    it.each([null, undefined, 'post'])(
      'update rejects the post %j',
      async (post) => {
        await expectClientRejection(() =>
          api.update(BREWER_ID, POST_ID, post as unknown as CreateNewsBlogPost),
        );
      },
    );

    it.each([null, undefined, 'filters'])(
      'search rejects the filters %j',
      async (filters) => {
        await expectClientRejection(() =>
          api.search({}, filters as unknown as BlogPostSearchFilters),
        );
      },
    );

    it.each([null, undefined])(
      'search sends no page params for the page %j',
      (page) => {
        const request = searchRequest(page as unknown as BlogPostPageQuery);
        expect(request.params.keys()).toEqual([]);
      },
    );

    it('search drops filters that have the wrong type instead of throwing', () => {
      const filters = {
        tag: 7,
        includeDrafts: 'yes',
      } as unknown as BlogPostSearchFilters;
      expect(searchRequest({}, filters).body).toEqual({});
    });
  });

  it('has no patch method, because the API has no PATCH for posts', () => {
    expect('patch' in api).toBe(false);
  });
});
