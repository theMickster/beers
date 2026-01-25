import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  type TestRequest,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '@beers/shared/data-access';
import { BrewerDetailStore } from './brewer-detail.store';
import type { Brewer } from './brewer.models';
import { BrewerStore } from './brewer.store';

const BASE_URL = 'https://api.test/api/v1';
const GUID_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const GUID_B = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';
const GUID_C = '3f2504e0-4f89-11d3-9a0c-0305e82c3303';

function makeBrewer(brewerId: string, name = `Brewer ${brewerId}`): Brewer {
  return {
    brewerId,
    name,
    foundedIn: 1990,
    headquarters: 'Portland, OR',
    website: 'https://acme.test',
    isDeletable: true,
    breweryType: { id: 't1', name: 'Micro' },
    rating: null,
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

describe('BrewerDetailStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BrewerDetailStore>;
  let brewerStore: InstanceType<typeof BrewerStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BrewerDetailStore);
    brewerStore = TestBed.inject(BrewerStore);
  });

  afterEach(() => httpMock.verify());

  const expectBrewer = (id: string): TestRequest =>
    httpMock.expectOne(`${BASE_URL}/brewers/${id}`);

  /** Puts brewers into the directory's store the way the directory does. */
  function cacheInDirectory(...brewers: Brewer[]): void {
    brewerStore.loadAll();
    httpMock.expectOne(`${BASE_URL}/brewers`).flush(brewers);
  }

  describe('a brewer that is not cached', () => {
    it('is pending until the GET returns, then holds the brewer', () => {
      store.load(GUID_A);

      expect(store.isPending()).toBe(true);
      expect(store.brewer()).toBeNull();
      const request = expectBrewer(GUID_A);
      expect(request.request.method).toBe('GET');

      request.flush(makeBrewer(GUID_A, 'Odell'));

      expect(store.isFulfilled()).toBe(true);
      expect(store.brewer()?.name).toBe('Odell');
      expect(store.notFound()).toBe(false);
    });

    it('is not found when the API answers 404', () => {
      store.load(GUID_A);
      expectBrewer(GUID_A).flush(['Brewer not found.'], {
        status: 404,
        statusText: 'Not Found',
      });

      expect(store.notFound()).toBe(true);
      expect(store.brewer()).toBeNull();
    });

    it('reports a server failure as an error, not as not found, and recovers on the next load', () => {
      store.load(GUID_A);
      expectBrewer(GUID_A).flush('boom', {
        status: 500,
        statusText: 'Server Error',
      });

      expect(store.error()).not.toBeNull();
      expect(store.notFound()).toBe(false);
      expect(store.brewer()).toBeNull();

      store.load(GUID_A);
      expect(store.isPending()).toBe(true);
      expectBrewer(GUID_A).flush(makeBrewer(GUID_A, 'Odell'));

      expect(store.error()).toBeNull();
      expect(store.brewer()?.name).toBe('Odell');
    });
  });

  // A component trusts the data only when this key equals its route, so it must always be current.
  describe('the recorded brewer id', () => {
    it('is the lowercase id from the last load, from the first moment of it', () => {
      store.load(GUID_A.toUpperCase());

      expect(store.brewerId()).toBe(GUID_A);
      httpMock.expectOne(() => true).flush(makeBrewer(GUID_A));
    });

    it('moves to the next brewer at once, before its answer, with the old brewer gone', () => {
      store.load(GUID_A);
      expectBrewer(GUID_A).flush(makeBrewer(GUID_A, 'Alpha'));

      store.load(GUID_B);

      expect(store.brewerId()).toBe(GUID_B);
      expect(store.brewer()).toBeNull();
      expect(store.isPending()).toBe(true);
      expectBrewer(GUID_B).flush(makeBrewer(GUID_B));
    });

    it('is recorded for an id that is not a GUID too', () => {
      store.load('Junk');

      expect(store.brewerId()).toBe('junk');
    });
  });

  describe('an id that cannot be a brewer', () => {
    it('is not found without calling the API', () => {
      store.load('not-a-guid');

      expect(store.notFound()).toBe(true);
      expect(store.brewer()).toBeNull();
      httpMock.expectNone(() => true);
    });

    it('clears a previously loaded brewer', () => {
      store.load(GUID_A);
      expectBrewer(GUID_A).flush(makeBrewer(GUID_A));

      store.load('not-a-guid');

      expect(store.brewer()).toBeNull();
      expect(store.notFound()).toBe(true);
    });
  });

  describe('switching brewers', () => {
    // A cancelled request cannot be flushed, so cancellation is what guarantees A never lands.
    it('cancels the first request so only the second brewer is shown', () => {
      store.load(GUID_A);
      const first = expectBrewer(GUID_A);

      store.load(GUID_B);
      const second = expectBrewer(GUID_B);

      expect(first.cancelled).toBe(true);
      second.flush(makeBrewer(GUID_B, 'Second'));
      expect(store.brewer()?.name).toBe('Second');
    });

    it('cancels an in-flight request when the next id is invalid', () => {
      store.load(GUID_A);
      const first = expectBrewer(GUID_A);

      store.load('not-a-guid');

      expect(first.cancelled).toBe(true);
      expect(store.brewer()).toBeNull();
      expect(store.notFound()).toBe(true);
    });
  });

  describe('a brewer the directory already loaded', () => {
    it('shows the cached brewer as fulfilled at once, then replaces it with fresh data', () => {
      cacheInDirectory(makeBrewer(GUID_A, 'Cached'));

      store.load(GUID_A);

      expect(store.brewer()?.name).toBe('Cached');
      expect(store.isFulfilled()).toBe(true);

      expectBrewer(GUID_A).flush(makeBrewer(GUID_A, 'Fresh'));
      expect(store.brewer()?.name).toBe('Fresh');
      expect(store.isFulfilled()).toBe(true);
    });

    it('finds the cached brewer when the id is typed in uppercase', () => {
      cacheInDirectory(makeBrewer(GUID_A, 'Cached'));

      store.load(GUID_A.toUpperCase());

      expect(store.brewer()?.name).toBe('Cached');
      httpMock.expectOne(() => true).flush(makeBrewer(GUID_A, 'Fresh'));
    });

    // A failed refetch must not leave the cached copy next to an error.
    it.each([
      [404, 'Not Found', true],
      [500, 'Server Error', false],
    ])(
      'drops the cached brewer when the refetch fails with status %i',
      (status, statusText, notFound) => {
        cacheInDirectory(makeBrewer(GUID_A, 'Cached'));

        store.load(GUID_A);
        expect(store.brewer()).not.toBeNull();
        expectBrewer(GUID_A).flush(['Refetch failed.'], { status, statusText });

        expect(store.notFound()).toBe(notFound);
        expect(store.error()).not.toBeNull();
        expect(store.brewer()).toBeNull();
      },
    );
  });

  describe('the directory store', () => {
    it('keeps its entities and status exactly as they were through successful and failed loads', () => {
      cacheInDirectory(
        makeBrewer(GUID_A, 'Alpha'),
        makeBrewer(GUID_C, 'Gamma'),
      );
      const entities = brewerStore.entities();
      const entityMap = brewerStore.entityMap();

      store.load(GUID_A);
      expectBrewer(GUID_A).flush(
        makeBrewer(GUID_A, 'Fresher than the directory'),
      );
      store.load(GUID_B);
      expectBrewer(GUID_B).flush('boom', {
        status: 500,
        statusText: 'Server Error',
      });
      store.load('not-a-guid');

      expect(brewerStore.entities()).toBe(entities);
      expect(brewerStore.entityMap()).toBe(entityMap);
      expect(brewerStore.isFulfilled()).toBe(true);
      expect(brewerStore.error()).toBeNull();
    });
  });
});
