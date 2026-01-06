import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  type TestRequest,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import { MetadataStore } from './metadata.store';

const BASE_URL = 'https://api.test/api/v1';
const PATHS = ['beerTypes', 'beerStyles', 'beerCategories', 'breweryTypes'];

describe('MetadataStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof MetadataStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(MetadataStore);
  });

  afterEach(() => httpMock.verify());

  function expectRequest(path: string): TestRequest {
    return httpMock.expectOne(`${BASE_URL}/${path}`);
  }

  function flushAll(): void {
    for (const path of PATHS) {
      expectRequest(path).flush([{ id: `${path}-id`, name: `${path}-name` }]);
    }
  }

  it('starts idle with empty lists', () => {
    expect(store.isIdle()).toBe(true);
    expect(store.error()).toBeNull();
    expect(store.beerTypes()).toEqual([]);
    expect(store.beerStyles()).toEqual([]);
    expect(store.beerCategories()).toEqual([]);
    expect(store.breweryTypes()).toEqual([]);
  });

  it('issues four GETs and populates each list', async () => {
    const loading = store.load();
    expect(store.isPending()).toBe(true);
    flushAll();
    await loading;

    expect(store.isFulfilled()).toBe(true);
    expect(store.beerTypes()).toEqual([
      { id: 'beerTypes-id', name: 'beerTypes-name' },
    ]);
    expect(store.beerStyles()).toEqual([
      { id: 'beerStyles-id', name: 'beerStyles-name' },
    ]);
    expect(store.beerCategories()).toEqual([
      { id: 'beerCategories-id', name: 'beerCategories-name' },
    ]);
    expect(store.breweryTypes()).toEqual([
      { id: 'breweryTypes-id', name: 'breweryTypes-name' },
    ]);
  });

  it('does not refetch once fulfilled', async () => {
    const loading = store.load();
    flushAll();
    await loading;

    await store.load();
    httpMock.expectNone(() => true);
  });

  it('makes one set of requests for concurrent loads while pending', async () => {
    const first = store.load();
    const second = store.load();
    flushAll();
    await Promise.all([first, second]);

    expect(store.isFulfilled()).toBe(true);
  });

  it('maps a 404 on one endpoint to an empty list and still fulfills', async () => {
    const loading = store.load();
    for (const path of PATHS) {
      const request = expectRequest(path);
      if (path === 'beerStyles') {
        request.flush('No styles found', {
          status: 404,
          statusText: 'Not Found',
        });
      } else {
        request.flush([{ id: 'x', name: 'y' }]);
      }
    }
    await loading;

    expect(store.isFulfilled()).toBe(true);
    expect(store.beerStyles()).toEqual([]);
    expect(store.beerTypes()).toHaveLength(1);
  });

  it('records a server error, leaves lists empty, and retries on the next load', async () => {
    const failing = store.load();
    const requests = PATHS.map(expectRequest);
    for (const request of requests.slice(1)) {
      request.flush([{ id: 'x', name: 'y' }]);
    }
    // forkJoin cancels the remaining requests on the first error, so fail last.
    requests[0].flush('boom', { status: 500, statusText: 'Server Error' });
    await failing;

    const error = store.error();
    expect(error).toBeInstanceOf(ApiError);
    expect(error?.kind).toBe('server');
    expect(store.isFulfilled()).toBe(false);
    expect(store.isPending()).toBe(false);
    expect(store.beerTypes()).toEqual([]);
    expect(store.beerStyles()).toEqual([]);

    const retry = store.load();
    flushAll();
    await retry;

    expect(store.isFulfilled()).toBe(true);
    expect(store.error()).toBeNull();
    expect(store.beerTypes()).toHaveLength(1);
  });

  it('reports a network failure as a network error', async () => {
    const loading = store.load();
    const requests = PATHS.map(expectRequest);
    for (const request of requests.slice(0, -1)) {
      request.flush([]);
    }
    // forkJoin cancels the remaining requests on the first error, so fail last.
    requests[3].error(new ProgressEvent('error'));
    await loading;

    expect(store.error()?.kind).toBe('network');
    expect(store.isFulfilled()).toBe(false);
  });
});
