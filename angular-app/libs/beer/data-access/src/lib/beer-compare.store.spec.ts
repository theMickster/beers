import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '@beers/shared/data-access';
import { BeerCompareStore } from './beer-compare.store';
import type { Beer } from './models';

const BASE_URL = 'https://api.test/api/v1';
const GUID_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const GUID_B = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';
const GUID_C = '3f2504e0-4f89-11d3-9a0c-0305e82c3303';

function makeBeer(beerId: string, name: string): Beer {
  return {
    beerId,
    brewerId: 'br-1',
    name,
    description: '',
    image: '',
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

describe('BeerCompareStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BeerCompareStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BeerCompareStore);
  });

  afterEach(() => httpMock.verify());

  const expectBeer = (id: string) =>
    httpMock.expectOne(`${BASE_URL}/beers/${id}`);

  describe('loading beers', () => {
    it('fetches each beer once, stays pending until all answer, then exposes the columns', () => {
      store.load([GUID_A, GUID_B]);

      expect(store.ids()).toEqual([GUID_A, GUID_B]);
      expect(store.isPending()).toBe(true);
      const first = expectBeer(GUID_A);
      const second = expectBeer(GUID_B);
      expect(first.request.method).toBe('GET');

      first.flush(makeBeer(GUID_A, 'Alpha'));
      expect(store.isPending()).toBe(true);
      expect(store.columns()).toEqual([]);

      second.flush(makeBeer(GUID_B, 'Bravo'));

      expect(store.isFulfilled()).toBe(true);
      expect(store.columns().map((c) => [c.beerId, c.beer?.name])).toEqual([
        [GUID_A, 'Alpha'],
        [GUID_B, 'Bravo'],
      ]);
      expect(store.columns().every((c) => c.error === null)).toBe(true);
    });

    it('keeps the requested order when the answers arrive in another order', () => {
      store.load([GUID_A, GUID_B, GUID_C]);
      const [a, b, c] = [GUID_A, GUID_B, GUID_C].map(expectBeer);

      c.flush(makeBeer(GUID_C, 'Charlie'));
      a.flush(makeBeer(GUID_A, 'Alpha'));
      b.flush(makeBeer(GUID_B, 'Bravo'));

      expect(store.columns().map((c) => c.beer?.name)).toEqual([
        'Alpha',
        'Bravo',
        'Charlie',
      ]);
    });
  });

  // A deleted beer must not take the whole comparison down with it.
  describe('a beer that fails', () => {
    it.each([
      [404, 'Not Found'],
      [500, 'Server Error'],
    ])(
      'keeps its column with the status %i error and still shows the others',
      (status, statusText) => {
        store.load([GUID_A, GUID_B]);
        expectBeer(GUID_A).flush(makeBeer(GUID_A, 'Alpha'));
        expectBeer(GUID_B).flush(['Beer not found.'], { status, statusText });

        expect(store.isFulfilled()).toBe(true);
        expect(store.error()).toBeNull();
        const [alpha, failed] = store.columns();
        expect(alpha.beer?.name).toBe('Alpha');
        expect(alpha.error).toBeNull();
        expect(failed.beerId).toBe(GUID_B);
        expect(failed.beer).toBeNull();
        expect(failed.error?.status).toBe(status);
      },
    );

    it('still fulfills when every beer fails', () => {
      store.load([GUID_A, GUID_B]);
      expectBeer(GUID_A).flush('x', { status: 500, statusText: 'Error' });
      expectBeer(GUID_B).flush('x', { status: 404, statusText: 'Not Found' });

      expect(store.isFulfilled()).toBe(true);
      expect(store.columns().map((c) => c.beer)).toEqual([null, null]);
    });
  });

  describe('loading again', () => {
    it('shows no columns from the first comparison while the second loads', () => {
      store.load([GUID_A, GUID_B]);
      expectBeer(GUID_A).flush(makeBeer(GUID_A, 'Alpha'));
      expectBeer(GUID_B).flush(makeBeer(GUID_B, 'Bravo'));

      store.load([GUID_B, GUID_C]);

      expect(store.ids()).toEqual([GUID_B, GUID_C]);
      expect(store.columns()).toEqual([]);
      expect(store.isPending()).toBe(true);
      expectBeer(GUID_B).flush(makeBeer(GUID_B, 'Bravo'));
      expectBeer(GUID_C).flush(makeBeer(GUID_C, 'Charlie'));
      expect(store.columns().map((c) => c.beerId)).toEqual([GUID_B, GUID_C]);
    });

    // A cancelled request cannot be flushed, so cancellation is what keeps the first set out.
    it('cancels the requests of the first comparison so they never land', () => {
      store.load([GUID_A, GUID_B]);
      const first = [expectBeer(GUID_A), expectBeer(GUID_B)];

      store.load([GUID_C]);

      expect(first.map((r) => r.cancelled)).toEqual([true, true]);
      expectBeer(GUID_C).flush(makeBeer(GUID_C, 'Charlie'));
      expect(store.columns().map((c) => c.beerId)).toEqual([GUID_C]);
    });

    it('recovers after a failure and fetches again on a repeat load of the same ids', () => {
      store.load([GUID_A]);
      expectBeer(GUID_A).flush('x', { status: 500, statusText: 'Error' });

      store.load([GUID_A]);
      expect(store.isPending()).toBe(true);
      expectBeer(GUID_A).flush(makeBeer(GUID_A, 'Alpha'));

      expect(store.columns()[0].beer?.name).toBe('Alpha');
      expect(store.columns()[0].error).toBeNull();
    });
  });

  describe('no ids', () => {
    it('is idle with no columns and makes no request', () => {
      store.load([]);

      expect(store.isIdle()).toBe(true);
      expect(store.ids()).toEqual([]);
      expect(store.columns()).toEqual([]);
      httpMock.expectNone(() => true);
    });

    it('clears a loaded comparison and cancels one in flight', () => {
      store.load([GUID_A]);
      const pending = expectBeer(GUID_A);

      store.load([]);

      expect(pending.cancelled).toBe(true);
      expect(store.isIdle()).toBe(true);
      expect(store.columns()).toEqual([]);
    });
  });
});
