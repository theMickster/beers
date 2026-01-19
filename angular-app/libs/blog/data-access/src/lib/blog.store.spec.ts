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
import { BlogApiService } from './blog-api.service';
import type {
  BlogPostPageQuery,
  BlogPostSearchFilters,
  CreateNewsBlogPost,
  NewsBlogPost,
} from './blog.models';
import { BlogStore } from './blog.store';

const BASE_URL = 'https://api.test/api/v1';
const BREWER_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BREWER_B = '9b2c4a10-1f5e-4c1d-8a3b-7d6e5f4a3b21';
const P1 = '00000000-0000-0000-0000-000000000001';
const P2 = '00000000-0000-0000-0000-000000000002';
const P3 = '00000000-0000-0000-0000-000000000003';
const POST_WITH_LETTERS = 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d';

const postsUrl = (brewerId: string) => `${BASE_URL}/brewers/${brewerId}/posts`;
const postUrl = (brewerId: string, postId: string) =>
  `${postsUrl(brewerId)}/${postId}`;
const SEARCH_URL = `${BASE_URL}/posts/search`;

function makePost(
  newsBlogPostId: string,
  brewerId = BREWER_A,
  title = `Post ${newsBlogPostId}`,
): NewsBlogPost {
  return {
    newsBlogPostId,
    brewerId,
    title,
    body: 'Body',
    postType: 'TextPost',
    tags: [],
    imageUrls: [],
    eventDate: null,
    eventLocation: null,
    publishedDate: '2026-01-01T00:00:00Z',
    author: {
      id: brewerId,
      name: 'Acme Brewing',
      website: 'https://acme.test',
    },
    isDeletable: true,
  };
}

function makePage(
  posts: NewsBlogPost[],
  overrides: Partial<PagedResult<NewsBlogPost>> = {},
): PagedResult<NewsBlogPost> {
  return {
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    totalRecords: posts.length,
    results: posts,
    ...overrides,
  };
}

const writePayload: CreateNewsBlogPost = {
  title: 'Grand opening',
  body: 'Doors open at noon.',
  postType: 'TextPost',
  tags: ['news'],
  imageUrls: [],
  eventDate: null,
  eventLocation: null,
  publishedDate: null,
};

const created = { status: 201, statusText: 'Created' };
const noContent = { status: 204, statusText: 'No Content' };

describe('BlogStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BlogStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BlogStore);
  });

  afterEach(() => httpMock.verify());

  const idsOf = () => store.ids().map(String);
  const resultIds = () => store.results().map((post) => post.newsBlogPostId);

  function expectSearch(): TestRequest {
    return httpMock.expectOne((r) => r.url === SEARCH_URL);
  }

  const notFound = { status: 404, statusText: 'Not Found' };
  const serverError = { status: 500, statusText: 'Server Error' };

  it('starts idle, empty and without an active scope', () => {
    expect(store.isIdle()).toBe(true);
    expect(store.error()).toBeNull();
    expect(store.activeScope()).toBeNull();
    expect(store.entities()).toEqual([]);
    expect(store.results()).toEqual([]);
    expect(store.totalRecords()).toBe(0);
  });

  describe('listByBrewer', () => {
    it('replaces entities, keeps the paged results untouched and sets the brewer scope', () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1)]));

      store.listByBrewer(BREWER_A);
      expect(store.isPending()).toBe(true);
      httpMock
        .expectOne(postsUrl(BREWER_A))
        .flush([makePost(P2), makePost(P3)]);

      expect(store.isFulfilled()).toBe(true);
      expect(store.activeScope()).toBe(BREWER_A);
      expect(idsOf()).toEqual([P2, P3]);
      expect(store.results()).toEqual([]);
    });

    it('stays fulfilled for a brewer without posts', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([]);

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.entities()).toEqual([]);
    });

    it('records a 404 as a client ApiError, not an empty list', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush('None', notFound);

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.status).toBe(404);
    });

    it('sets a server ApiError on 500 and recovers on the next call', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush('boom', serverError);
      expect(store.error()).toBeInstanceOf(ApiError);
      expect(store.error()?.kind).toBe('server');

      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);
      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(idsOf()).toEqual([P1]);
    });

    it('sets a network ApiError on status 0', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).error(new ProgressEvent('error'));
      expect(store.error()?.kind).toBe('network');
    });

    it('records an invalid brewer id as an error without a request', () => {
      store.listByBrewer('not-a-guid');

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('search', () => {
    it('fills results, paging metadata and entities under the feed scope', () => {
      store.search({ pageNumber: 1, pageSize: 2 }, { tag: 'ipa' });
      expect(store.isPending()).toBe(true);
      expect(store.activeScope()).toBe('all');

      const request = expectSearch();
      expect(request.request.params.get('PageSize')).toBe('2');
      expect(request.request.body).toEqual({ tag: 'ipa' });
      request.flush(
        makePage([makePost(P1), makePost(P2, BREWER_B)], {
          pageSize: 2,
          totalPages: 2,
          totalRecords: 3,
          hasNextPage: true,
        }),
      );

      expect(store.isFulfilled()).toBe(true);
      expect(resultIds()).toEqual([P1, P2]);
      expect(store.totalRecords()).toBe(3);
      expect(store.totalPages()).toBe(2);
      expect(store.hasNextPage()).toBe(true);
      expect(idsOf()).toEqual([P1, P2]);
    });

    it('upserts a second page without duplicating entities', () => {
      store.search({ pageNumber: 1 }, {});
      expectSearch().flush(makePage([makePost(P1), makePost(P2)]));

      store.search({ pageNumber: 2 }, {});
      expectSearch().flush(
        makePage([makePost(P2, BREWER_A, 'Renamed'), makePost(P3)], {
          pageNumber: 2,
        }),
      );

      expect(idsOf()).toEqual([P1, P2, P3]);
      expect(store.entityMap()[P2]?.title).toBe('Renamed');
      expect(resultIds()).toEqual([P2, P3]);
      expect(store.pageNumber()).toBe(2);
    });

    it('keeps the feed scope when the filters name a brewer', () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1)]));

      store.search({}, { brewerId: BREWER_A });

      expect(store.activeScope()).toBe('all');
      expect(idsOf()).toEqual([P1]);
      expectSearch().flush(makePage([makePost(P1)]));
    });

    it('stays fulfilled for an empty result', () => {
      store.search({}, {});
      expectSearch().flush(makePage([], { totalPages: 0 }));

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.results()).toEqual([]);
    });

    it('cancels a pending search when a newer one starts', () => {
      store.search({ pageNumber: 1 }, {});
      const first = expectSearch();
      store.search({ pageNumber: 2 }, {});
      const second = expectSearch();

      expect(first.cancelled).toBe(true);
      second.flush(makePage([makePost(P3)], { pageNumber: 2 }));
      expect(resultIds()).toEqual([P3]);
    });

    it('sets a server ApiError on 500 and recovers on the next search', () => {
      store.search({}, {});
      expectSearch().flush('boom', serverError);
      expect(store.error()?.kind).toBe('server');

      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1)]));
      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(idsOf()).toEqual([P1]);
    });

    it('sets a network ApiError on status 0', () => {
      store.search({}, {});
      expectSearch().error(new ProgressEvent('error'));
      expect(store.error()?.kind).toBe('network');
    });

    it('records an invalid filter as an error without a request', () => {
      store.search({}, { brewerId: 'x' });

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('getById', () => {
    it('upserts and exposes the entity', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      store.getById(BREWER_A, P2);
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(postUrl(BREWER_A, P2)).flush(makePost(P2));

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([P1, P2]);
    });

    it('clears the feed because it uses the brewer scope', () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1)], { totalRecords: 5 }));

      store.getById(BREWER_A, P1);

      expect(store.activeScope()).toBe(BREWER_A);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.entities()).toEqual([]);
      httpMock.expectOne(postUrl(BREWER_A, P1)).flush(makePost(P1));
    });

    it('records a 404 as a client ApiError and leaves entities alone', () => {
      store.getById(BREWER_A, P1);
      httpMock.expectOne(postUrl(BREWER_A, P1)).flush(null, notFound);

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.status).toBe(404);
      expect(store.entities()).toEqual([]);
    });

    it('records an invalid post id as an error without a request', () => {
      store.getById(BREWER_A, 'not-a-guid');

      expect(store.error()?.messages).toEqual([
        'Post id must be a valid GUID.',
      ]);
      httpMock.expectNone(() => true);
    });
  });

  describe('switching scope', () => {
    it('empties the store when moving from the feed to a brewer', () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1)], { totalRecords: 7 }));

      store.listByBrewer(BREWER_B);

      expect(store.activeScope()).toBe(BREWER_B);
      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.pageNumber()).toBe(1);
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(postsUrl(BREWER_B)).flush([makePost(P2, BREWER_B)]);
      expect(idsOf()).toEqual([P2]);
    });

    it('empties the store when moving from a brewer to the feed', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      store.search({}, {});

      expect(store.activeScope()).toBe('all');
      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
      expectSearch().flush(makePage([makePost(P2, BREWER_B)]));
      expect(idsOf()).toEqual([P2]);
    });

    it('empties the store when moving between two brewers', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      store.listByBrewer(BREWER_B);

      expect(store.activeScope()).toBe(BREWER_B);
      expect(store.entities()).toEqual([]);
      httpMock.expectOne(postsUrl(BREWER_B)).flush([makePost(P2, BREWER_B)]);
      expect(idsOf()).toEqual([P2]);
    });

    it('clears a previous error', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush('boom', serverError);
      expect(store.error()).not.toBeNull();

      store.search({}, {});

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      expectSearch().flush(makePage([]));
    });

    it('does not clear anything when the same brewer is used again', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      store.getById(BREWER_A, P2);

      expect(idsOf()).toEqual([P1]);
      httpMock.expectOne(postUrl(BREWER_A, P2)).flush(makePost(P2));
    });

    it('ignores a late response from the previous scope and keeps the status untouched', () => {
      store.listByBrewer(BREWER_A);
      const late = httpMock.expectOne(postsUrl(BREWER_A));
      store.search({}, {});
      const current = expectSearch();

      late.flush([makePost(P1)]);

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
      expect(store.activeScope()).toBe('all');

      current.flush(makePage([makePost(P2, BREWER_B)]));
      expect(idsOf()).toEqual([P2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a late feed response after moving to a brewer', () => {
      store.search({}, {});
      const late = expectSearch();
      store.listByBrewer(BREWER_A);
      const current = httpMock.expectOne(postsUrl(BREWER_A));

      late.flush(makePage([makePost(P1)], { totalRecords: 9 }));

      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.isPending()).toBe(true);

      current.flush([makePost(P2)]);
      expect(idsOf()).toEqual([P2]);
    });

    it('ignores a late error from the previous scope', () => {
      store.listByBrewer(BREWER_A);
      const late = httpMock.expectOne(postsUrl(BREWER_A));
      store.search({}, {});
      const current = expectSearch();

      late.flush('boom', serverError);

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      current.flush(makePage([]));
      expect(store.isFulfilled()).toBe(true);
    });

    it('isolates every kind of read from a scope switch', async () => {
      store.search({}, {});
      const lateSearch = expectSearch();
      store.getById(BREWER_A, P3);
      const lateOne = httpMock.expectOne(postUrl(BREWER_A, P3));
      store.listByBrewer(BREWER_A);
      const lateList = httpMock.expectOne(postsUrl(BREWER_A));

      // A write is the one call that switches scope without cancelling any of the reads.
      const switching = store.create(BREWER_B, writePayload);
      const current = httpMock.expectOne(postsUrl(BREWER_B));

      lateSearch.flush(makePage([makePost(P2)], { totalRecords: 9 }));
      lateOne.flush(makePost(P3));
      lateList.flush([makePost(P1)]);

      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.isPending()).toBe(true);

      current.flush(makePost(P1, BREWER_B), created);
      await switching;
      expect(idsOf()).toEqual([P1]);
      expect(store.activeScope()).toBe(BREWER_B);
      expect(store.isFulfilled()).toBe(true);
    });

    it('keeps the feed scope for a write to any brewer', async () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1, BREWER_B)]));

      const pending = store.create(BREWER_A, writePayload);
      httpMock
        .expectOne(postsUrl(BREWER_A))
        .flush(makePost(P2, BREWER_A), created);
      await pending;

      expect(store.activeScope()).toBe('all');
      expect(idsOf()).toEqual([P1, P2]);
      expect(resultIds()).toEqual([P1]);
    });

    it('switches from one brewer to another for a write', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      const pending = store.create(BREWER_B, writePayload);

      expect(store.activeScope()).toBe(BREWER_B);
      expect(store.entities()).toEqual([]);
      httpMock
        .expectOne(postsUrl(BREWER_B))
        .flush(makePost(P2, BREWER_B), created);
      await pending;
      expect(idsOf()).toEqual([P2]);
    });

    it('ignores a write response from the previous scope', async () => {
      const pending = store.create(BREWER_A, writePayload);
      const late = httpMock.expectOne(postsUrl(BREWER_A));

      store.search({}, {});
      const current = expectSearch();

      late.flush(makePost(P1), created);
      await pending;

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);

      current.flush(makePage([]));
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a write error from the previous scope', async () => {
      const pending = store.delete(BREWER_A, P1);
      const late = httpMock.expectOne(postUrl(BREWER_A, P1));

      store.search({}, {});
      const current = expectSearch();

      late.flush('boom', serverError);
      await pending;

      expect(store.error()).toBeNull();
      current.flush(makePage([]));
      expect(store.isFulfilled()).toBe(true);
    });
  });

  describe('create', () => {
    it('adds the created post as an entity and does not touch results or totals', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      const pending = store.create(BREWER_A, writePayload);
      expect(store.isPending()).toBe(true);
      const request = httpMock.expectOne(postsUrl(BREWER_A));
      expect(request.request.method).toBe('POST');
      request.flush(makePost(P2), created);
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([P1, P2]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
    });

    it('resolves and exposes 400 messages from a string array', async () => {
      const pending = store.create(BREWER_A, writePayload);
      httpMock
        .expectOne(postsUrl(BREWER_A))
        .flush(['Brewer does not exist', 'Title is too long'], {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual([
        'Brewer does not exist',
        'Title is too long',
      ]);
      expect(store.entities()).toEqual([]);
    });

    it('leaves the store in an error state without a request for invalid input', async () => {
      await expect(
        store.create(BREWER_A, { ...writePayload, title: '' }),
      ).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('update', () => {
    it('replaces the entity and the results row without adjusting totals', async () => {
      store.search({}, {});
      expectSearch().flush(
        makePage([makePost(P1), makePost(P2)], { totalRecords: 12 }),
      );

      const pending = store.update(BREWER_A, P1, writePayload);
      const request = httpMock.expectOne(postUrl(BREWER_A, P1));
      expect(request.request.method).toBe('PUT');
      request.flush(makePost(P1, BREWER_A, 'Edited'));
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(store.entityMap()[P1]?.title).toBe('Edited');
      expect(store.results().map((post) => post.title)).toEqual([
        'Edited',
        `Post ${P2}`,
      ]);
      expect(store.totalRecords()).toBe(12);
    });

    it('leaves the entity alone and exposes the message on a 400', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      const pending = store.update(BREWER_A, P1, writePayload);
      httpMock
        .expectOne(postUrl(BREWER_A, P1))
        .flush(['Brewer does not exist'], {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual(['Brewer does not exist']);
      expect(store.entityMap()[P1]?.title).toBe(`Post ${P1}`);
    });

    it('errors without a request for an invalid post id', async () => {
      await expect(
        store.update(BREWER_A, 'not-a-guid', writePayload),
      ).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('delete', () => {
    it('removes the entity from entities and results without adjusting totals', async () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1), makePost(P2)]));

      const pending = store.delete(BREWER_A, P1);
      const request = httpMock.expectOne(postUrl(BREWER_A, P1));
      expect(request.request.method).toBe('DELETE');
      request.flush(null, noContent);
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([P2]);
      expect(resultIds()).toEqual([P2]);
      expect(store.totalRecords()).toBe(2);
    });

    it('keeps the entity and exposes the message on a 400', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      const pending = store.delete(BREWER_A, P1);
      httpMock.expectOne(postUrl(BREWER_A, P1)).flush('Post is not deletable', {
        status: 400,
        statusText: 'Bad Request',
      });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual(['Post is not deletable']);
      expect(idsOf()).toEqual([P1]);
    });

    it('keeps the entity on a 404', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      const pending = store.delete(BREWER_A, P1);
      httpMock.expectOne(postUrl(BREWER_A, P1)).flush('Not found', notFound);
      await pending;

      expect(store.error()?.status).toBe(404);
      expect(idsOf()).toEqual([P1]);
    });

    it('errors without a request for an invalid post id', async () => {
      await expect(
        store.delete(BREWER_A, 'not-a-guid'),
      ).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('reset', () => {
    it('clears the active scope, entities, page and status', () => {
      store.search({}, {});
      expectSearch().flush(makePage([makePost(P1)], { totalRecords: 4 }));

      store.reset();

      expect(store.activeScope()).toBeNull();
      expect(store.entities()).toEqual([]);
      expect(store.results()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.isIdle()).toBe(true);
    });

    it('ignores a response that arrives after the reset', () => {
      store.listByBrewer(BREWER_A);
      const late = httpMock.expectOne(postsUrl(BREWER_A));

      store.reset();
      late.flush([makePost(P1)]);

      expect(store.entities()).toEqual([]);
      expect(store.isIdle()).toBe(true);
    });

    it('ignores an older response after reset() followed by the same scope', () => {
      store.listByBrewer(BREWER_A);
      const stale = httpMock.expectOne(postsUrl(BREWER_A));
      store.reset();
      store.getById(BREWER_A, P2);
      const current = httpMock.expectOne(postUrl(BREWER_A, P2));

      stale.flush([makePost(P1)]);

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
      current.flush(makePost(P2));
      expect(idsOf()).toEqual([P2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a write response that resolves after reset() and the same brewer again', async () => {
      const pending = store.create(BREWER_A, writePayload);
      const stale = httpMock.expectOne(postsUrl(BREWER_A));
      store.reset();
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A));

      stale.flush(makePost(P1), created);
      await pending;

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
    });
  });

  describe('stale responses after the store was cleared', () => {
    it('ignores an older list(A) response after A, feed, A', () => {
      store.listByBrewer(BREWER_A);
      const stale = httpMock.expectOne(postsUrl(BREWER_A));
      store.search({}, {});
      expectSearch();
      store.getById(BREWER_A, P3);
      const current = httpMock.expectOne(postUrl(BREWER_A, P3));

      stale.flush([makePost(P1)]);

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);

      current.flush(makePost(P3));
      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([P3]);
    });

    it('ignores an older list(A) error after A, feed, A and leaves the status pending', () => {
      store.listByBrewer(BREWER_A);
      const stale = httpMock.expectOne(postsUrl(BREWER_A));
      store.search({}, {});
      expectSearch();
      store.getById(BREWER_A, P3);
      const current = httpMock.expectOne(postUrl(BREWER_A, P3));

      stale.flush('boom', serverError);

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      current.flush(makePost(P3));
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a create(A) response that resolves after A, feed, A', async () => {
      const pending = store.create(BREWER_A, writePayload);
      const stale = httpMock.expectOne(postsUrl(BREWER_A));
      store.search({}, {});
      expectSearch();
      store.listByBrewer(BREWER_A);
      const current = httpMock.expectOne(postsUrl(BREWER_A));

      stale.flush(makePost(P1), created);
      await pending;

      expect(store.entities()).toEqual([]);
      expect(store.isPending()).toBe(true);
      current.flush([makePost(P2)]);
      expect(idsOf()).toEqual([P2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores an update(A) response that resolves after A, feed, A', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);
      const pending = store.update(BREWER_A, P1, writePayload);
      const stale = httpMock.expectOne(postUrl(BREWER_A, P1));
      store.search({}, {});
      expectSearch();
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);

      stale.flush(makePost(P1, BREWER_A, 'Edited'));
      await pending;

      expect(store.entityMap()[P1]?.title).toBe(`Post ${P1}`);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a delete(A) response that resolves after A, feed, A', async () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);
      const pending = store.delete(BREWER_A, P1);
      const stale = httpMock.expectOne(postUrl(BREWER_A, P1));
      store.search({}, {});
      expectSearch();
      store.listByBrewer(BREWER_A);
      httpMock
        .expectOne(postsUrl(BREWER_A))
        .flush([makePost(P1), makePost(P2)]);
      expect(store.isFulfilled()).toBe(true);

      stale.flush(null, noContent);
      await pending;

      expect(idsOf()).toEqual([P1, P2]);
      expect(store.isFulfilled()).toBe(true);
    });

    it('ignores a delete(A) error that resolves after A, feed, A', async () => {
      const pending = store.delete(BREWER_A, P1);
      const stale = httpMock.expectOne(postUrl(BREWER_A, P1));
      store.search({}, {});
      expectSearch();
      store.listByBrewer(BREWER_A);
      const current = httpMock.expectOne(postsUrl(BREWER_A));

      stale.flush('boom', serverError);
      await pending;

      expect(store.error()).toBeNull();
      expect(store.isPending()).toBe(true);
      current.flush([]);
      expect(store.isFulfilled()).toBe(true);
    });
  });

  describe('ids are case-insensitive', () => {
    const upperBrewer = BREWER_A.toUpperCase();
    const upperPost = POST_WITH_LETTERS.toUpperCase();

    it('does not treat a different letter case of the active brewer as a switch', () => {
      store.listByBrewer(upperBrewer);
      httpMock.expectOne(postsUrl(upperBrewer)).flush([makePost(P1)]);
      expect(store.activeScope()).toBe(BREWER_A);

      store.listByBrewer(BREWER_A);

      expect(idsOf()).toEqual([P1]);
      expect(store.activeScope()).toBe(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);
    });

    it('removes the entity and the results row when deleted by an uppercase id', async () => {
      store.search({}, {});
      expectSearch().flush(
        makePage([makePost(POST_WITH_LETTERS), makePost(P2)]),
      );

      const pending = store.delete(upperBrewer, upperPost);
      httpMock
        .expectOne(postUrl(upperBrewer, upperPost))
        .flush(null, noContent);
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([P2]);
      expect(resultIds()).toEqual([P2]);
    });

    it('keys entities by the lowercase post id even when the server sends uppercase', () => {
      store.listByBrewer(BREWER_A);
      httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(upperPost)]);

      expect(idsOf()).toEqual([POST_WITH_LETTERS]);
    });
  });

  describe('malformed runtime input', () => {
    const asString = (value: unknown) => value as string;

    it.each([null, undefined, 42])(
      'listByBrewer(%j) records a client error and the store keeps working',
      (brewerId) => {
        store.listByBrewer(asString(brewerId));

        expect(store.error()?.kind).toBe('client');
        expect(store.error()?.status).toBe(400);
        httpMock.expectNone(() => true);

        store.listByBrewer(BREWER_A);
        httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([P1]);
      },
    );

    it.each([null, undefined, 'filters'])(
      'search with the filters %j records a client error and the store keeps working',
      (filters) => {
        store.search({}, filters as unknown as BlogPostSearchFilters);

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);

        store.search({}, {});
        expectSearch().flush(makePage([makePost(P1)]));
        expect(store.isFulfilled()).toBe(true);
        expect(resultIds()).toEqual([P1]);
      },
    );

    it('search treats a missing page as no page params and keeps working', () => {
      store.search(undefined as unknown as BlogPostPageQuery, {});
      const request = expectSearch();
      expect(request.request.params.keys()).toEqual([]);
      request.flush(makePage([makePost(P1)]));
      expect(store.isFulfilled()).toBe(true);
    });

    it.each([null, undefined, 'post'])(
      'create with the post %j resolves with a client error and the store keeps working',
      async (post) => {
        await expect(
          store.create(BREWER_A, post as unknown as CreateNewsBlogPost),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        expect(store.error()?.status).toBe(400);
        httpMock.expectNone(() => true);

        const pending = store.create(BREWER_A, writePayload);
        httpMock.expectOne(postsUrl(BREWER_A)).flush(makePost(P1), created);
        await pending;
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([P1]);
      },
    );

    it.each([null, undefined, 42])(
      'delete with the post id %j resolves with a client error and the store keeps working',
      async (postId) => {
        await expect(
          store.delete(BREWER_A, asString(postId)),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);

        const pending = store.delete(BREWER_A, P1);
        httpMock.expectOne(postUrl(BREWER_A, P1)).flush(null, noContent);
        await pending;
        expect(store.isFulfilled()).toBe(true);
      },
    );
  });

  describe('an API call that throws synchronously', () => {
    const boom = () => {
      throw new TypeError('boom');
    };

    it.each(['listByBrewer', 'search', 'getById'] as const)(
      '%s lands in error() and the rxMethod keeps working',
      (method) => {
        const spy = vi
          .spyOn(TestBed.inject(BlogApiService), method)
          .mockImplementationOnce(boom);

        if (method === 'listByBrewer') store.listByBrewer(BREWER_A);
        if (method === 'search') store.search({}, {});
        if (method === 'getById') store.getById(BREWER_A, P1);

        expect(store.error()).toBeInstanceOf(ApiError);
        expect(spy).toHaveBeenCalledTimes(1);

        store.listByBrewer(BREWER_A);
        httpMock.expectOne(postsUrl(BREWER_A)).flush([makePost(P1)]);
        expect(store.isFulfilled()).toBe(true);
      },
    );

    it.each(['create', 'update', 'delete'] as const)(
      '%s resolves with error() set instead of throwing',
      async (method) => {
        vi.spyOn(TestBed.inject(BlogApiService), method).mockImplementationOnce(
          boom,
        );

        const call = {
          create: () => store.create(BREWER_A, writePayload),
          update: () => store.update(BREWER_A, P1, writePayload),
          delete: () => store.delete(BREWER_A, P1),
        }[method];

        await expect(call()).resolves.toBeUndefined();

        expect(store.error()).toBeInstanceOf(ApiError);
      },
    );
  });

  describe('no patch', () => {
    it('exposes no patch method, because the API has no PATCH for posts', () => {
      expect('patch' in store).toBe(false);
    });
  });
});
