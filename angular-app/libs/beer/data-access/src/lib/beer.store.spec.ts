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
import { BeerStore } from './beer.store';
import type { Beer, CreateBeer, UpdateBeer } from './models';

const BASE_URL = 'https://api.test/api/v1';

function makeBeer(beerId: string, name = `Beer ${beerId}`): Beer {
  return {
    beerId,
    brewerId: 'br-1',
    name,
    description: 'desc',
    image: 'img.png',
    sku: `sku-${beerId}`,
    isDeletable: true,
    rating: null,
    pricing: null,
    brewer: { id: 'br-1', name: 'Brewer', website: 'https://b.test' },
    beerType: { id: 't1', name: 'Lager' },
    beerCategories: [],
    beerStyles: [],
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

function makePage(
  beers: Beer[],
  overrides: Partial<PagedResult<Beer>> = {},
): PagedResult<Beer> {
  return {
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    totalRecords: beers.length,
    results: beers,
    ...overrides,
  };
}

const writePayload: CreateBeer = {
  brewerId: 'br-1',
  name: 'New',
  description: 'desc',
  image: 'img.png',
  sku: 'sku-new',
  isDeletable: true,
  brewer: { id: 'br-1', name: 'Brewer', website: 'https://b.test' },
  beerTypeId: 't1',
  beerCategories: [],
  beerStyles: [],
};

describe('BeerStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BeerStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BeerStore);
  });

  afterEach(() => httpMock.verify());

  function expectSearch(): TestRequest {
    return httpMock.expectOne((r) => r.url === `${BASE_URL}/beers/search`);
  }

  const idsOf = () => store.ids().map(String);

  it('starts idle and empty', () => {
    expect(store.isIdle()).toBe(true);
    expect(store.error()).toBeNull();
    expect(store.results()).toEqual([]);
    expect(store.entities()).toEqual([]);
    expect(store.totalRecords()).toBe(0);
  });

  describe('loadPage', () => {
    it('populates results, paging metadata and entities keyed by beerId', () => {
      store.loadPage({ pageNumber: 1, pageSize: 2 });
      expect(store.isPending()).toBe(true);

      const request = expectSearch();
      expect(request.request.params.get('PageNumber')).toBe('1');
      expect(request.request.params.get('PageSize')).toBe('2');
      expect(request.request.body).toEqual({});
      request.flush(
        makePage([makeBeer('a'), makeBeer('b')], {
          pageSize: 2,
          totalPages: 2,
          totalRecords: 3,
          hasNextPage: true,
        }),
      );

      expect(store.isFulfilled()).toBe(true);
      expect(store.results().map((b) => b.beerId)).toEqual(['a', 'b']);
      expect(store.totalRecords()).toBe(3);
      expect(store.totalPages()).toBe(2);
      expect(store.hasNextPage()).toBe(true);
      expect(idsOf()).toEqual(['a', 'b']);
      expect(store.entityMap()['a']?.name).toBe('Beer a');
    });

    it('upserts a second page without duplicating entities', () => {
      store.loadPage({ pageNumber: 1 });
      expectSearch().flush(makePage([makeBeer('a'), makeBeer('b')]));

      store.loadPage({ pageNumber: 2 });
      expectSearch().flush(
        makePage([makeBeer('b', 'Renamed'), makeBeer('c')], { pageNumber: 2 }),
      );

      expect(idsOf()).toEqual(['a', 'b', 'c']);
      expect(store.entityMap()['b']?.name).toBe('Renamed');
      expect(store.results().map((b) => b.beerId)).toEqual(['b', 'c']);
      expect(store.pageNumber()).toBe(2);
    });

    it('cancels a pending request when a newer one starts', () => {
      store.loadPage({ pageNumber: 1 });
      const first = expectSearch();
      store.loadPage({ pageNumber: 2 });
      const second = expectSearch();

      expect(first.cancelled).toBe(true);
      second.flush(makePage([makeBeer('z')], { pageNumber: 2 }));
      expect(store.results().map((b) => b.beerId)).toEqual(['z']);
    });

    it('stays fulfilled for an empty result', () => {
      store.loadPage({});
      expectSearch().flush(makePage([], { totalPages: 0 }));

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.results()).toEqual([]);
    });

    it('sets a server ApiError on 500 and recovers on the next request', () => {
      store.loadPage({});
      expectSearch().flush('boom', { status: 500, statusText: 'Server Error' });

      expect(store.error()).toBeInstanceOf(ApiError);
      expect(store.error()?.kind).toBe('server');
      expect(store.error()?.status).toBe(500);

      store.loadPage({});
      expectSearch().flush(makePage([makeBeer('a')]));
      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(idsOf()).toEqual(['a']);
    });

    it('sets a network ApiError on status 0', () => {
      store.loadPage({});
      expectSearch().error(new ProgressEvent('error'));

      expect(store.error()?.kind).toBe('network');
      expect(store.error()?.status).toBe(0);
    });
  });

  describe('search', () => {
    it('sends the filters and stores the results', () => {
      store.search({
        page: { pageNumber: 1 },
        filters: { name: 'ipa', brewerName: '' },
      });

      const request = expectSearch();
      expect(request.request.body).toEqual({ name: 'ipa' });
      request.flush(makePage([makeBeer('a')]));

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual(['a']);
    });
  });

  describe('getById', () => {
    it('upserts and exposes the entity', () => {
      store.getById('a');
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(`${BASE_URL}/beers/a`).flush(makeBeer('a'));

      expect(store.isFulfilled()).toBe(true);
      expect(store.entityMap()['a']?.beerId).toBe('a');
    });

    it('records a 404 as a client ApiError', () => {
      store.getById('missing');
      httpMock
        .expectOne(`${BASE_URL}/beers/missing`)
        .flush(null, { status: 404, statusText: 'Not Found' });

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.status).toBe(404);
      expect(store.entities()).toEqual([]);
    });

    it('records a 500 and a network failure', () => {
      store.getById('a');
      httpMock
        .expectOne(`${BASE_URL}/beers/a`)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      expect(store.error()?.kind).toBe('server');

      store.getById('a');
      httpMock
        .expectOne(`${BASE_URL}/beers/a`)
        .error(new ProgressEvent('error'));
      expect(store.error()?.kind).toBe('network');
    });
  });

  describe('create', () => {
    it('adds the created beer as an entity', async () => {
      const pending = store.create(writePayload);
      expect(store.isPending()).toBe(true);
      const request = httpMock.expectOne(`${BASE_URL}/beers`);
      expect(request.request.method).toBe('POST');
      request.flush(makeBeer('new'), { status: 201, statusText: 'Created' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual(['new']);
    });

    it('resolves and exposes 400 messages from a string array', async () => {
      const pending = store.create(writePayload);
      httpMock
        .expectOne(`${BASE_URL}/beers`)
        .flush(['Name is required', 'Sku is required'], {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.messages).toEqual([
        'Name is required',
        'Sku is required',
      ]);
      expect(store.entities()).toEqual([]);
    });
  });

  describe('update', () => {
    it('updates the entity and the matching entry in results', async () => {
      store.loadPage({});
      expectSearch().flush(makePage([makeBeer('a'), makeBeer('b')]));

      const update: UpdateBeer = {
        ...writePayload,
        beerId: 'a',
        name: 'Edited',
      };
      const pending = store.update(update);
      const request = httpMock.expectOne(`${BASE_URL}/beers/a`);
      expect(request.request.method).toBe('PUT');
      request.flush(makeBeer('a', 'Edited'));
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(store.entityMap()['a']?.name).toBe('Edited');
      expect(store.results().map((b) => b.name)).toEqual(['Edited', 'Beer b']);
    });

    it('does not add to results when the beer is not on the page', async () => {
      const pending = store.update({ ...writePayload, beerId: 'x' });
      httpMock.expectOne(`${BASE_URL}/beers/x`).flush(makeBeer('x'));
      await pending;

      expect(store.results()).toEqual([]);
      expect(idsOf()).toEqual(['x']);
    });

    it('resolves with an error on failure', async () => {
      const pending = store.update({ ...writePayload, beerId: 'x' });
      httpMock
        .expectOne(`${BASE_URL}/beers/x`)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      await expect(pending).resolves.toBeUndefined();
      expect(store.error()?.kind).toBe('server');
    });
  });

  describe('delete', () => {
    it('removes the entity and drops it from results', async () => {
      store.loadPage({});
      expectSearch().flush(makePage([makeBeer('a'), makeBeer('b')]));

      const pending = store.delete('a');
      const request = httpMock.expectOne(`${BASE_URL}/beers/a`);
      expect(request.request.method).toBe('DELETE');
      request.flush(null, { status: 204, statusText: 'No Content' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual(['b']);
      expect(store.results().map((b) => b.beerId)).toEqual(['b']);
      expect(store.totalRecords()).toBe(2);
    });

    it('resolves with an error on 404 and keeps the entity', async () => {
      store.getById('a');
      httpMock.expectOne(`${BASE_URL}/beers/a`).flush(makeBeer('a'));

      const pending = store.delete('a');
      httpMock
        .expectOne(`${BASE_URL}/beers/a`)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      expect(idsOf()).toEqual(['a']);
    });
  });
});
