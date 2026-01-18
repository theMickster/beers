import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import type {
  Brewer,
  BrewerPatch,
  CreateBrewer,
  UpdateBrewer,
} from './brewer.models';
import { BrewerApiService } from './brewer-api.service';
import { BrewerStore } from './brewer.store';

const BASE_URL = 'https://api.test/api/v1';
const A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const B = '9b2c4a10-1f5e-4c1d-8a3b-7d6e5f4a3b21';
const C = 'c0ffee00-1f5e-4c1d-8a3b-7d6e5f4a3b22';
const BREWERY_TYPE_ID = 'b7e0a3c1-5d2f-4a8b-9c6e-1f3d5a7b9c0d';

function makeBrewer(brewerId: string, name = `Brewer ${brewerId}`): Brewer {
  return {
    brewerId,
    name,
    foundedIn: 1990,
    headquarters: 'Portland, OR',
    website: 'https://acme.test',
    isDeletable: true,
    breweryType: { id: BREWERY_TYPE_ID, name: 'Micro' },
    rating: null,
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

const writePayload: CreateBrewer = {
  name: 'New',
  foundedIn: 2000,
  headquarters: 'Austin, TX',
  website: 'https://new.test',
  breweryType: { id: BREWERY_TYPE_ID, name: 'Micro' },
};

describe('BrewerStore', () => {
  let httpMock: HttpTestingController;
  let store: InstanceType<typeof BrewerStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    store = TestBed.inject(BrewerStore);
  });

  afterEach(() => httpMock.verify());

  const idsOf = () => store.ids().map(String);
  const brewersUrl = `${BASE_URL}/brewers`;

  it('starts idle and empty', () => {
    expect(store.isIdle()).toBe(true);
    expect(store.error()).toBeNull();
    expect(store.entities()).toEqual([]);
  });

  describe('loadAll', () => {
    it('goes pending then fulfilled and stores entities keyed by brewerId', () => {
      store.loadAll();
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A), makeBrewer(B)]);

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([A, B]);
      expect(store.entityMap()[A]?.name).toBe(`Brewer ${A}`);
    });

    it('stays fulfilled and empty on a 404', () => {
      store.loadAll();
      httpMock
        .expectOne(brewersUrl)
        .flush('No brewers', { status: 404, statusText: 'Not Found' });

      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(store.entities()).toEqual([]);
    });

    it('replaces everything, dropping ids the server no longer returns', () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A), makeBrewer(B)]);

      store.loadAll();
      httpMock
        .expectOne(brewersUrl)
        .flush([makeBrewer(B, 'Renamed'), makeBrewer(C)]);

      expect(idsOf()).toEqual([B, C]);
      expect(store.entityMap()[B]?.name).toBe('Renamed');
    });

    it('cancels a pending request when a newer one starts', () => {
      store.loadAll();
      const first = httpMock.expectOne(brewersUrl);
      store.loadAll();
      const second = httpMock.expectOne(brewersUrl);

      expect(first.cancelled).toBe(true);
      second.flush([makeBrewer(C)]);
      expect(idsOf()).toEqual([C]);
    });

    it('sets a server ApiError on 500 and recovers on the next call', () => {
      store.loadAll();
      httpMock
        .expectOne(brewersUrl)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      expect(store.error()).toBeInstanceOf(ApiError);
      expect(store.error()?.kind).toBe('server');

      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A)]);
      expect(store.isFulfilled()).toBe(true);
      expect(store.error()).toBeNull();
      expect(idsOf()).toEqual([A]);
    });

    it('sets a network ApiError on status 0 and recovers on the next call', () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).error(new ProgressEvent('error'));
      expect(store.error()?.kind).toBe('network');

      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A)]);
      expect(store.isFulfilled()).toBe(true);
    });
  });

  describe('getById', () => {
    it('upserts without dropping other entities', () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A)]);

      store.getById(B);
      expect(store.isPending()).toBe(true);
      httpMock.expectOne(`${brewersUrl}/${B}`).flush(makeBrewer(B));

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([A, B]);
    });

    it('replaces an existing entity', () => {
      store.getById(A);
      httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A));
      store.getById(A);
      httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A, 'Renamed'));

      expect(idsOf()).toEqual([A]);
      expect(store.entityMap()[A]?.name).toBe('Renamed');
    });

    it('records a 404 as a client ApiError and leaves entities alone', () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A)]);

      store.getById(B);
      httpMock
        .expectOne(`${brewersUrl}/${B}`)
        .flush(null, { status: 404, statusText: 'Not Found' });

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.status).toBe(404);
      expect(idsOf()).toEqual([A]);
    });

    it('records an invalid id as an error without a request', () => {
      store.getById('not-a-guid');

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.messages).toEqual([
        'Brewer id must be a valid GUID.',
      ]);
      httpMock.expectNone(() => true);
    });
  });

  describe('create', () => {
    it('adds the created brewer as an entity', async () => {
      const pending = store.create(writePayload);
      expect(store.isPending()).toBe(true);
      const request = httpMock.expectOne(brewersUrl);
      expect(request.request.method).toBe('POST');
      request.flush(makeBrewer(A, 'New'), {
        status: 201,
        statusText: 'Created',
      });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([A]);
    });

    it('resolves and exposes 400 messages from a string array', async () => {
      const pending = store.create(writePayload);
      httpMock
        .expectOne(brewersUrl)
        .flush(['Brewer must be unique', 'Website is required'], {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.messages).toEqual([
        'Brewer must be unique',
        'Website is required',
      ]);
      expect(store.entities()).toEqual([]);
    });

    it('resolves and exposes a plain 400 string message', async () => {
      const pending = store.create(writePayload);
      httpMock.expectOne(brewersUrl).flush('"Brewer must be unique"', {
        status: 400,
        statusText: 'Bad Request',
      });
      await expect(pending).resolves.toBeUndefined();
      expect(store.error()?.messages).toEqual(['Brewer must be unique']);
    });

    it('leaves the store in an error state without a request for invalid input', async () => {
      await expect(
        store.create({ ...writePayload, name: '' }),
      ).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.messages).toEqual(['Name is required.']);
      expect(store.entities()).toEqual([]);
      httpMock.expectNone(() => true);
    });
  });

  describe('update (PUT)', () => {
    it('replaces the entity with the response, including the server-owned rating', async () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A), makeBrewer(B)]);

      const pending = store.update(A, { ...writePayload, brewerId: A });
      const request = httpMock.expectOne(`${brewersUrl}/${A}`);
      expect(request.request.method).toBe('PUT');
      request.flush({
        ...makeBrewer(A, 'Edited'),
        rating: { average: 4, reviewCount: 3 },
      });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(store.entityMap()[A]?.name).toBe('Edited');
      expect(store.entityMap()[A]?.rating).toEqual({
        average: 4,
        reviewCount: 3,
      });
      expect(idsOf()).toEqual([A, B]);
    });

    it('handles the 400 the API returns for a missing brewer', async () => {
      const pending = store.update(A, { ...writePayload, brewerId: A });
      httpMock
        .expectOne(`${brewersUrl}/${A}`)
        .flush('Brewer not found', { status: 400, statusText: 'Bad Request' });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.status).toBe(400);
      expect(store.error()?.messages).toEqual(['Brewer not found']);
      expect(store.entities()).toEqual([]);
    });

    it('errors without a request when the body id differs from the route id', async () => {
      await store.update(A, { ...writePayload, brewerId: B });

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('patch (PATCH)', () => {
    it('sends JSON Patch and replaces the entity from the response', async () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A)]);

      const pending = store.patch(A, { name: 'Patched' });
      const request = httpMock.expectOne(`${brewersUrl}/${A}`);
      expect(request.request.method).toBe('PATCH');
      expect(request.request.headers.get('Content-Type')).toBe(
        'application/json-patch+json',
      );
      request.flush({ ...makeBrewer(A, 'Patched by server'), foundedIn: 1991 });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(store.entityMap()[A]?.name).toBe('Patched by server');
      expect(store.entityMap()[A]?.foundedIn).toBe(1991);
    });

    it('resolves with a client error on 404', async () => {
      const pending = store.patch(A, { name: 'Patched' });
      httpMock
        .expectOne(`${brewersUrl}/${A}`)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.status).toBe(404);
      expect(store.entities()).toEqual([]);
    });

    it('resolves with the 400 messages from the server', async () => {
      const pending = store.patch(A, { name: 'Patched' });
      httpMock
        .expectOne(`${brewersUrl}/${A}`)
        .flush(['Invalid patch'], { status: 400, statusText: 'Bad Request' });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual(['Invalid patch']);
    });

    it('errors without a request for an empty patch', async () => {
      await expect(store.patch(A, {})).resolves.toBeUndefined();

      expect(store.error()?.messages).toEqual([
        'At least one field to change is required.',
      ]);
      httpMock.expectNone(() => true);
    });
  });

  describe('delete', () => {
    it('removes the entity', async () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A), makeBrewer(B)]);

      const pending = store.delete(A);
      const request = httpMock.expectOne(`${brewersUrl}/${A}`);
      expect(request.request.method).toBe('DELETE');
      request.flush(null, { status: 204, statusText: 'No Content' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([B]);
    });

    it('keeps the entity and exposes the message when the brewer is not deletable', async () => {
      store.getById(A);
      httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A));

      const pending = store.delete(A);
      httpMock
        .expectOne(`${brewersUrl}/${A}`)
        .flush('Brewer is not deletable', {
          status: 400,
          statusText: 'Bad Request',
        });
      await expect(pending).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      expect(store.error()?.messages).toEqual(['Brewer is not deletable']);
      expect(idsOf()).toEqual([A]);
    });

    it('keeps the entity on a 404', async () => {
      store.getById(A);
      httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A));

      const pending = store.delete(A);
      httpMock
        .expectOne(`${brewersUrl}/${A}`)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await pending;

      expect(store.error()?.status).toBe(404);
      expect(idsOf()).toEqual([A]);
    });

    it('errors without a request for an invalid id', async () => {
      await expect(store.delete('not-a-guid')).resolves.toBeUndefined();

      expect(store.error()?.kind).toBe('client');
      httpMock.expectNone(() => true);
    });
  });

  describe('ids are case-insensitive', () => {
    it('removes the entity when deleted by an uppercase id', async () => {
      store.loadAll();
      httpMock.expectOne(brewersUrl).flush([makeBrewer(A), makeBrewer(B)]);

      const pending = store.delete(A.toUpperCase());
      httpMock
        .expectOne(`${brewersUrl}/${A.toUpperCase()}`)
        .flush(null, { status: 204, statusText: 'No Content' });
      await pending;

      expect(store.isFulfilled()).toBe(true);
      expect(idsOf()).toEqual([B]);
    });

    it('keys entities by the lowercase id even when the server sends uppercase', () => {
      store.getById(A);
      httpMock
        .expectOne(`${brewersUrl}/${A}`)
        .flush(makeBrewer(A.toUpperCase()));
      store.getById(A);
      httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A, 'Renamed'));

      expect(idsOf()).toEqual([A]);
      expect(store.entityMap()[A]?.name).toBe('Renamed');
    });
  });

  describe('malformed runtime input', () => {
    it.each([null, undefined, 42])(
      'getById(%j) records a client error and the store keeps working',
      (brewerId) => {
        store.getById(brewerId as unknown as string);

        expect(store.error()?.kind).toBe('client');
        expect(store.error()?.status).toBe(400);
        httpMock.expectNone(() => true);

        store.getById(A);
        httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A));
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([A]);
      },
    );

    it.each([null, undefined, 'brewer'])(
      'create(%j) resolves with a client error and the store keeps working',
      async (body) => {
        await expect(
          store.create(body as unknown as CreateBrewer),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        expect(store.error()?.status).toBe(400);
        httpMock.expectNone(() => true);

        const pending = store.create(writePayload);
        httpMock
          .expectOne(brewersUrl)
          .flush(makeBrewer(A), { status: 201, statusText: 'Created' });
        await pending;
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([A]);
      },
    );

    it.each([null, undefined, 'brewer'])(
      'update with the body %j resolves with a client error',
      async (body) => {
        await expect(
          store.update(A, body as unknown as UpdateBrewer),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);
      },
    );

    it.each([null, undefined, 42])(
      'update with the route id %j resolves with a client error',
      async (brewerId) => {
        await expect(
          store.update(brewerId as unknown as string, {
            ...writePayload,
            brewerId: A,
          }),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);
      },
    );

    it.each([null, undefined, 'name'])(
      'patch with the changes %j resolves with a client error',
      async (changes) => {
        await expect(
          store.patch(A, changes as unknown as BrewerPatch),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);
      },
    );

    it.each([null, undefined, 42])(
      'delete(%j) resolves with a client error and the store keeps working',
      async (brewerId) => {
        await expect(
          store.delete(brewerId as unknown as string),
        ).resolves.toBeUndefined();

        expect(store.error()?.kind).toBe('client');
        httpMock.expectNone(() => true);

        store.getById(A);
        httpMock.expectOne(`${brewersUrl}/${A}`).flush(makeBrewer(A));
        const pending = store.delete(A);
        httpMock
          .expectOne(`${brewersUrl}/${A}`)
          .flush(null, { status: 204, statusText: 'No Content' });
        await pending;
        expect(store.isFulfilled()).toBe(true);
        expect(idsOf()).toEqual([]);
      },
    );
  });

  describe('an API call that throws synchronously', () => {
    const boom = () => {
      throw new TypeError('boom');
    };

    it.each(['list', 'getById'] as const)(
      '%s lands in error() and the rxMethod keeps working',
      (method) => {
        vi.spyOn(
          TestBed.inject(BrewerApiService),
          method,
        ).mockImplementationOnce(boom);

        if (method === 'list') store.loadAll();
        else store.getById(A);
        expect(store.error()).toBeInstanceOf(ApiError);

        store.loadAll();
        httpMock.expectOne(brewersUrl).flush([makeBrewer(A)]);
        expect(store.isFulfilled()).toBe(true);
      },
    );

    it('create resolves with error() set instead of throwing', async () => {
      vi.spyOn(
        TestBed.inject(BrewerApiService),
        'create',
      ).mockImplementationOnce(boom);

      await expect(store.create(writePayload)).resolves.toBeUndefined();

      expect(store.error()).toBeInstanceOf(ApiError);
    });
  });
});
