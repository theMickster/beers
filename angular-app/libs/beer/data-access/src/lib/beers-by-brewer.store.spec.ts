import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  type TestRequest,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, type PagedResult } from '@beers/shared/data-access';
import { BeerStore } from './beer.store';
import { BeersByBrewerStore } from './beers-by-brewer.store';
import type { Beer } from './models';

const BASE_URL = 'https://api.test/api/v1';
const BREWER_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BREWER_B = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';

function makeBeer(beerId: string, brewerId: string): Beer {
  return {
    beerId,
    brewerId,
    name: `Beer ${beerId}`,
    description: '',
    image: '',
    sku: `sku-${beerId}`,
    isDeletable: true,
    rating: null,
    pricing: null,
    brewer: { id: brewerId, name: 'Brewer', website: 'https://b.test' },
    beerType: { id: 't1', name: 'Lager' },
    beerCategories: [],
    beerStyles: [],
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

function makePage(
  beers: Beer[],
  totalRecords = beers.length,
): PagedResult<Beer> {
  return {
    pageNumber: 1,
    pageSize: 50,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    totalRecords,
    results: beers,
  };
}

describe('BeersByBrewerStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BeersByBrewerStore>;
  let beerStore: InstanceType<typeof BeerStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BeersByBrewerStore);
    beerStore = TestBed.inject(BeerStore);
  });

  afterEach(() => httpMock.verify());

  const expectSearch = (): TestRequest =>
    httpMock.expectOne((r) => r.url === `${BASE_URL}/beers/search`);

  describe('loading a brewer', () => {
    it('asks for one page of 50 filtered by the brewer, and is pending until it answers', () => {
      store.load(BREWER_A);

      expect(store.isPending()).toBe(true);
      const request = expectSearch();
      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual({ brewerId: BREWER_A });
      expect(request.request.params.get('PageNumber')).toBe('1');
      expect(request.request.params.get('PageSize')).toBe('50');
    });

    it('holds the beers and the real total once the page arrives', () => {
      store.load(BREWER_A);

      expectSearch().flush(
        makePage([makeBeer('b1', BREWER_A), makeBeer('b2', BREWER_A)], 61),
      );

      expect(store.isFulfilled()).toBe(true);
      expect(store.beers().map((b) => b.beerId)).toEqual(['b1', 'b2']);
      expect(store.totalRecords()).toBe(61);
    });

    // Zero beers is a normal answer, not a failure.
    it('is fulfilled with no beers when the brewer has none', () => {
      store.load(BREWER_A);

      expectSearch().flush(makePage([]));

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.beers()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
    });

    it('reports a failure as an error and recovers on the next load', () => {
      store.load(BREWER_A);
      expectSearch().flush('boom', { status: 500, statusText: 'Server Error' });

      expect(store.error()?.status).toBe(500);
      expect(store.beers()).toEqual([]);

      store.load(BREWER_A);
      expect(store.isPending()).toBe(true);
      expectSearch().flush(makePage([makeBeer('b1', BREWER_A)]));

      expect(store.error()).toBeNull();
      expect(store.beers()).toHaveLength(1);
    });
  });

  // A component trusts the data only when this key equals its route, so it must always be current.
  describe('the recorded brewer id', () => {
    it('is the lowercase id from the first moment of the load', () => {
      store.load(BREWER_A.toUpperCase());

      expect(store.brewerId()).toBe(BREWER_A);
      expectSearch().flush(makePage([]));
    });

    it('moves to the next brewer at once, with the first brewer beers gone', () => {
      store.load(BREWER_A);
      expectSearch().flush(makePage([makeBeer('b1', BREWER_A)], 9));

      store.load(BREWER_B);

      expect(store.brewerId()).toBe(BREWER_B);
      expect(store.beers()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
      expect(store.isPending()).toBe(true);
      expectSearch().flush(makePage([makeBeer('b2', BREWER_B)]));
    });
  });

  describe('switching brewers', () => {
    // A cancelled request cannot be flushed, so cancellation is what keeps brewer A out.
    it('cancels the first request so its beers never land under the second brewer', () => {
      store.load(BREWER_A);
      const first = expectSearch();

      store.load(BREWER_B);
      const second = expectSearch();

      expect(first.cancelled).toBe(true);
      second.flush(makePage([makeBeer('b2', BREWER_B)]));
      expect(store.beers().map((b) => b.beerId)).toEqual(['b2']);
    });
  });

  describe('the explorer store', () => {
    it('keeps its entities and status exactly as they were through successful and failed loads', () => {
      beerStore.loadPage({ pageNumber: 1 });
      expectSearch().flush(makePage([makeBeer('seed', BREWER_B)]));
      const entities = beerStore.entities();
      const entityMap = beerStore.entityMap();

      store.load(BREWER_A);
      expectSearch().flush(makePage([makeBeer('b1', BREWER_A)]));
      store.load(BREWER_B);
      expectSearch().flush('boom', { status: 500, statusText: 'Server Error' });

      expect(beerStore.entities()).toBe(entities);
      expect(beerStore.entityMap()).toBe(entityMap);
      expect(beerStore.isFulfilled()).toBe(true);
      expect(beerStore.error()).toBeNull();
    });
  });
});
