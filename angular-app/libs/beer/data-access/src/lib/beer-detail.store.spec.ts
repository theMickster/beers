import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  type TestRequest,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, type PagedResult } from '@beers/shared/data-access';
import { BeerDetailStore } from './beer-detail.store';
import { BeerStore } from './beer.store';
import type { Beer } from './models';

const BASE_URL = 'https://api.test/api/v1';
const GUID_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const GUID_B = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';

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

function makePage(beers: Beer[]): PagedResult<Beer> {
  return {
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    totalRecords: beers.length,
    results: beers,
  };
}

describe('BeerDetailStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BeerDetailStore>;
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
    store = TestBed.inject(BeerDetailStore);
    beerStore = TestBed.inject(BeerStore);
  });

  afterEach(() => httpMock.verify());

  const expectBeer = (id: string): TestRequest =>
    httpMock.expectOne(`${BASE_URL}/beers/${id}`);

  /** Puts beers into the explorer's store the way the explorer does. */
  function cacheInExplorer(...beers: Beer[]): void {
    beerStore.loadPage({ pageNumber: 1 });
    httpMock
      .expectOne((r) => r.url === `${BASE_URL}/beers/search`)
      .flush(makePage(beers));
  }

  describe('a beer that is not cached', () => {
    it('is pending until the GET returns, then holds the beer', () => {
      store.load(GUID_A);

      expect(store.isPending()).toBe(true);
      expect(store.beer()).toBeNull();
      const request = expectBeer(GUID_A);
      expect(request.request.method).toBe('GET');

      request.flush(makeBeer(GUID_A, 'Hazy Day'));

      expect(store.isFulfilled()).toBe(true);
      expect(store.beer()?.name).toBe('Hazy Day');
      expect(store.notFound()).toBe(false);
    });

    it('is not found when the API answers 404', () => {
      store.load(GUID_A);
      expectBeer(GUID_A).flush(['Beer not found.'], {
        status: 404,
        statusText: 'Not Found',
      });

      expect(store.notFound()).toBe(true);
      expect(store.beer()).toBeNull();
    });

    it('reports a server failure as an error, not as not found, and recovers on the next load', () => {
      store.load(GUID_A);
      expectBeer(GUID_A).flush('boom', {
        status: 500,
        statusText: 'Server Error',
      });

      expect(store.error()).not.toBeNull();
      expect(store.notFound()).toBe(false);
      expect(store.beer()).toBeNull();

      store.load(GUID_A);
      expect(store.isPending()).toBe(true);
      expectBeer(GUID_A).flush(makeBeer(GUID_A, 'Hazy Day'));

      expect(store.error()).toBeNull();
      expect(store.beer()?.name).toBe('Hazy Day');
    });
  });

  describe('an id that cannot be a beer', () => {
    it('is not found without calling the API', () => {
      store.load('not-a-guid');

      expect(store.notFound()).toBe(true);
      expect(store.beer()).toBeNull();
      httpMock.expectNone(() => true);
    });

    it('clears a previously loaded beer', () => {
      store.load(GUID_A);
      expectBeer(GUID_A).flush(makeBeer(GUID_A));

      store.load('not-a-guid');

      expect(store.beer()).toBeNull();
      expect(store.notFound()).toBe(true);
    });
  });

  describe('switching beers', () => {
    // A cancelled request cannot be flushed, so cancellation is what guarantees A never lands.
    it('cancels the first request so only the second beer is shown', () => {
      store.load(GUID_A);
      const first = expectBeer(GUID_A);

      store.load(GUID_B);
      const second = expectBeer(GUID_B);

      expect(first.cancelled).toBe(true);
      second.flush(makeBeer(GUID_B, 'Second'));
      expect(store.beer()?.name).toBe('Second');
    });

    it('cancels an in-flight request when the next id is invalid', () => {
      store.load(GUID_A);
      const first = expectBeer(GUID_A);

      store.load('not-a-guid');

      expect(first.cancelled).toBe(true);
      expect(store.beer()).toBeNull();
      expect(store.notFound()).toBe(true);
    });
  });

  describe('a beer the explorer already loaded', () => {
    it('shows the cached beer as fulfilled at once, then replaces it with fresh data', () => {
      cacheInExplorer(makeBeer(GUID_A, 'Cached'));

      store.load(GUID_A);

      expect(store.beer()?.name).toBe('Cached');
      expect(store.isFulfilled()).toBe(true);

      expectBeer(GUID_A).flush(makeBeer(GUID_A, 'Fresh'));
      expect(store.beer()?.name).toBe('Fresh');
      expect(store.isFulfilled()).toBe(true);
    });

    it('finds the cached beer when the id is typed in uppercase', () => {
      cacheInExplorer(makeBeer(GUID_A, 'Cached'));

      store.load(GUID_A.toUpperCase());

      expect(store.beer()?.name).toBe('Cached');
      httpMock.expectOne(() => true).flush(makeBeer(GUID_A, 'Fresh'));
    });

    // A failed refetch must not leave the cached copy next to an error.
    it.each([
      [404, 'Not Found', true],
      [500, 'Server Error', false],
    ])(
      'drops the cached beer when the refetch fails with status %i',
      (status, statusText, notFound) => {
        cacheInExplorer(makeBeer(GUID_A, 'Cached'));

        store.load(GUID_A);
        expect(store.beer()).not.toBeNull();
        expectBeer(GUID_A).flush(['Refetch failed.'], { status, statusText });

        expect(store.notFound()).toBe(notFound);
        expect(store.error()).not.toBeNull();
        expect(store.beer()).toBeNull();
      },
    );
  });

  describe('the explorer store', () => {
    it('is not written to by a successful or failed detail load', () => {
      store.load(GUID_A);
      expectBeer(GUID_A).flush(makeBeer(GUID_A));
      store.load(GUID_B);
      expectBeer(GUID_B).flush('boom', {
        status: 500,
        statusText: 'Server Error',
      });

      expect(beerStore.entities()).toEqual([]);
      expect(beerStore.isIdle()).toBe(true);
      expect(beerStore.error()).toBeNull();
    });
  });
});
