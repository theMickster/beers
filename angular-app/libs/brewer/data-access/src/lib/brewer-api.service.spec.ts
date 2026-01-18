import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import { firstValueFrom, type Observable } from 'rxjs';
import { BrewerApiService } from './brewer-api.service';
import type {
  Brewer,
  BrewerPatch,
  BrewerPatchOperation,
  CreateBrewer,
  UpdateBrewer,
} from './brewer.models';

const BASE_URL = 'https://api.test/api/v1';
const BREWER_ID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BREWERY_TYPE_ID = 'b7e0a3c1-5d2f-4a8b-9c6e-1f3d5a7b9c0d';
const BREWERY_TYPE_ID_2 = '1c9d8e7f-6a5b-4c3d-8e2f-0a1b2c3d4e5f';
const BREWER_URL = `${BASE_URL}/brewers/${BREWER_ID}`;

const createBrewer: CreateBrewer = {
  name: 'Acme Brewing',
  foundedIn: 1990,
  headquarters: 'Portland, OR',
  website: 'https://acme.test',
  breweryType: { id: BREWERY_TYPE_ID, name: 'Micro' },
};

const updateBrewer: UpdateBrewer = { ...createBrewer, brewerId: BREWER_ID };

function makeBrewer(): Brewer {
  return {
    ...createBrewer,
    brewerId: BREWER_ID,
    isDeletable: true,
    rating: { average: 4.5, reviewCount: 2 },
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
  };
}

describe('BrewerApiService', () => {
  let httpMock: HttpTestingController;
  let api: BrewerApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    api = TestBed.inject(BrewerApiService);
  });

  afterEach(() => httpMock.verify());

  async function expectClientError(
    result: Promise<unknown>,
    messageFragment: string,
  ): Promise<void> {
    const error: unknown = await result.catch((caught: unknown) => caught);
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).kind).toBe('client');
    expect((error as ApiError).status).toBe(400);
    expect((error as ApiError).messages.join(' ')).toContain(messageFragment);
  }

  describe('list', () => {
    it('GETs /brewers', async () => {
      const result = firstValueFrom(api.list());
      const request = httpMock.expectOne(`${BASE_URL}/brewers`);
      expect(request.request.method).toBe('GET');
      request.flush([makeBrewer()]);
      expect(await result).toHaveLength(1);
    });

    it('maps a 404 to an empty list', async () => {
      const result = firstValueFrom(api.list());
      httpMock
        .expectOne(`${BASE_URL}/brewers`)
        .flush('No brewers', { status: 404, statusText: 'Not Found' });
      expect(await result).toEqual([]);
    });

    it('propagates other errors', async () => {
      const result = firstValueFrom(api.list());
      httpMock
        .expectOne(`${BASE_URL}/brewers`)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      await expect(result).rejects.toMatchObject({ status: 500 });
    });
  });

  describe('getById', () => {
    it('GETs /brewers/{id}', () => {
      api.getById(BREWER_ID).subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(request.request.method).toBe('GET');
      request.flush(makeBrewer());
    });

    it('propagates a 404', async () => {
      const result = firstValueFrom(api.getById(BREWER_ID));
      httpMock
        .expectOne(BREWER_URL)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(result).rejects.toMatchObject({ status: 404 });
    });

    it('sends no request for an id that is not a GUID, so it never needs encoding', async () => {
      await expectClientError(
        firstValueFrom(api.getById('a/b c')),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('create', () => {
    it('POSTs exactly the five persisted fields', () => {
      const spread = { ...makeBrewer() };
      api.create(spread).subscribe();
      const request = httpMock.expectOne(`${BASE_URL}/brewers`);
      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(createBrewer);
      expect(Object.keys(request.request.body as object).sort()).toEqual([
        'breweryType',
        'foundedIn',
        'headquarters',
        'name',
        'website',
      ]);
      request.flush(makeBrewer(), { status: 201, statusText: 'Created' });
    });

    it('sends no request for invalid input', async () => {
      await expectClientError(
        firstValueFrom(api.create({ ...createBrewer, website: 'http://x' })),
        'https://',
      );
      httpMock.expectNone(() => true);
    });
  });

  describe('update (PUT)', () => {
    it('PUTs to a URL built from brewerId with the route id in the body', () => {
      api.update(BREWER_ID, updateBrewer).subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual({
        brewerId: BREWER_ID,
        ...createBrewer,
      });
      request.flush(makeBrewer());
    });

    it('strips rating and isDeletable from a spread Brewer', () => {
      api.update(BREWER_ID, makeBrewer()).subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(Object.keys(request.request.body as object).sort()).toEqual([
        'brewerId',
        'breweryType',
        'foundedIn',
        'headquarters',
        'name',
        'website',
      ]);
      request.flush(makeBrewer());
    });

    it('sends a plain JSON object, not a JSON Patch document', () => {
      api.update(BREWER_ID, updateBrewer).subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(Array.isArray(request.request.body)).toBe(false);
      expect(request.request.headers.has('Content-Type')).toBe(false);
      expect(request.request.detectContentTypeHeader()).toBe(
        'application/json',
      );
      request.flush(makeBrewer());
    });

    it('sends no request when the body id differs from the route id', async () => {
      await expectClientError(
        firstValueFrom(
          api.update('00000000-0000-0000-0000-000000000001', updateBrewer),
        ),
        'must match',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for invalid fields', async () => {
      await expectClientError(
        firstValueFrom(api.update(BREWER_ID, { ...updateBrewer, name: '' })),
        'Name is required',
      );
      httpMock.expectNone(() => true);
    });

    it('propagates the 400 the API returns for a missing brewer', async () => {
      const result = firstValueFrom(api.update(BREWER_ID, updateBrewer));
      httpMock
        .expectOne(BREWER_URL)
        .flush('Brewer not found', { status: 400, statusText: 'Bad Request' });
      await expect(result).rejects.toMatchObject({ status: 400 });
    });
  });

  describe('patch (PATCH)', () => {
    it('PATCHes JSON Patch replace operations with the json-patch content type', () => {
      api.patch(BREWER_ID, { name: 'Renamed' }).subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(request.request.method).toBe('PATCH');
      expect(request.request.headers.get('Content-Type')).toBe(
        'application/json-patch+json',
      );
      expect(request.request.body).toEqual([
        { op: 'replace', path: '/name', value: 'Renamed' },
      ]);
      request.flush(makeBrewer());
    });

    it('emits operations in allow-list order regardless of key order', () => {
      api
        .patch(BREWER_ID, {
          breweryType: { id: BREWERY_TYPE_ID_2, name: 'Macro' },
          website: 'https://new.test',
          headquarters: 'Austin, TX',
          foundedIn: 2001,
          name: 'Renamed',
        })
        .subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      const operations = request.request.body as BrewerPatchOperation[];
      expect(operations.map((operation) => operation.path)).toEqual([
        '/name',
        '/foundedIn',
        '/headquarters',
        '/website',
        '/breweryType',
      ]);
      expect(operations.every((operation) => operation.op === 'replace')).toBe(
        true,
      );
      request.flush(makeBrewer());
    });

    it('replaces breweryType whole, never by sub-path', () => {
      api
        .patch(BREWER_ID, {
          breweryType: { id: BREWERY_TYPE_ID_2, name: 'Macro' },
        })
        .subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(request.request.body).toEqual([
        {
          op: 'replace',
          path: '/breweryType',
          value: { id: BREWERY_TYPE_ID_2, name: 'Macro' },
        },
      ]);
      request.flush(makeBrewer());
    });

    it('skips fields that are undefined', () => {
      api
        .patch(BREWER_ID, { name: undefined, website: 'https://w.test' })
        .subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(request.request.body).toEqual([
        { op: 'replace', path: '/website', value: 'https://w.test' },
      ]);
      request.flush(makeBrewer());
    });

    it.each(['brewerId', 'isDeletable', 'rating', 'unknown'])(
      'sends no request, and so no /%s operation, when the key is cast in',
      async (key) => {
        const changes = { name: 'Renamed', [key]: 'x' } as BrewerPatch;
        await expectClientError(
          firstValueFrom(api.patch(BREWER_ID, changes)),
          `'${key}' cannot be patched`,
        );
        httpMock.expectNone(() => true);
      },
    );

    it('sends no request for an empty patch', async () => {
      await expectClientError(
        firstValueFrom(api.patch(BREWER_ID, {})),
        'At least one field',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for an invalid value', async () => {
      await expectClientError(
        firstValueFrom(api.patch(BREWER_ID, { foundedIn: 1499 })),
        'Founded in',
      );
      httpMock.expectNone(() => true);
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.patch('brewer-1', { name: 'Renamed' })),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });

    it('propagates a 404', async () => {
      const result = firstValueFrom(api.patch(BREWER_ID, { name: 'Renamed' }));
      httpMock
        .expectOne(BREWER_URL)
        .flush('Not found', { status: 404, statusText: 'Not Found' });
      await expect(result).rejects.toMatchObject({ status: 404 });
    });
  });

  describe('PUT versus PATCH', () => {
    it('uses distinct verbs and content types on the same URL', () => {
      api.update(BREWER_ID, updateBrewer).subscribe();
      api.patch(BREWER_ID, { name: 'Renamed' }).subscribe();

      const requests = httpMock.match(BREWER_URL);
      expect(requests.map((request) => request.request.method)).toEqual([
        'PUT',
        'PATCH',
      ]);
      expect(Array.isArray(requests[0]?.request.body)).toBe(false);
      expect(Array.isArray(requests[1]?.request.body)).toBe(true);
      requests.forEach((request) => request.flush(makeBrewer()));
    });
  });

  describe('delete', () => {
    it('DELETEs /brewers/{id}', () => {
      api.delete(BREWER_ID).subscribe();
      const request = httpMock.expectOne(BREWER_URL);
      expect(request.request.method).toBe('DELETE');
      request.flush(null, { status: 204, statusText: 'No Content' });
    });

    it('sends no request for a brewer id that is not a GUID', async () => {
      await expectClientError(
        firstValueFrom(api.delete('not-a-guid')),
        'valid GUID',
      );
      httpMock.expectNone(() => true);
    });
  });
});

describe('BrewerApiService with malformed runtime input', () => {
  let httpMock: HttpTestingController;
  let api: BrewerApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    api = TestBed.inject(BrewerApiService);
  });

  afterEach(() => httpMock.verify());

  async function expectClientRejection(request: () => Observable<unknown>) {
    const error: unknown = await firstValueFrom(request()).catch(
      (caught: unknown) => caught,
    );
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).kind).toBe('client');
    expect((error as ApiError).status).toBe(400);
    httpMock.expectNone(() => true);
  }

  const bad = [null, undefined, 42, {}] as unknown as string[];

  it.each(bad)('getById rejects the id %j', async (id) => {
    await expectClientRejection(() => api.getById(id));
  });

  it.each(bad)('delete rejects the id %j', async (id) => {
    await expectClientRejection(() => api.delete(id));
  });

  it.each([null, undefined, 'brewer'])(
    'create rejects the body %j',
    async (body) => {
      await expectClientRejection(() =>
        api.create(body as unknown as CreateBrewer),
      );
    },
  );

  it.each([null, undefined, 'brewer'])(
    'update rejects the body %j',
    async (body) => {
      await expectClientRejection(() =>
        api.update(BREWER_ID, body as unknown as UpdateBrewer),
      );
    },
  );

  it.each(bad)('update rejects the route id %j', async (id) => {
    await expectClientRejection(() => api.update(id, updateBrewer));
  });

  it.each([null, undefined, 'name'])(
    'patch rejects the changes %j',
    async (changes) => {
      await expectClientRejection(() =>
        api.patch(BREWER_ID, changes as unknown as BrewerPatch),
      );
    },
  );

  it.each(bad)('patch rejects the id %j', async (id) => {
    await expectClientRejection(() => api.patch(id, { name: 'Renamed' }));
  });

  it('rejects a brewery type id that is not a GUID', async () => {
    const breweryType = { id: 'bt-1', name: 'Micro' };
    await expectClientRejection(() =>
      api.create({ ...createBrewer, breweryType }),
    );
    await expectClientRejection(() => api.patch(BREWER_ID, { breweryType }));
  });
});

describe('brewer patch types', () => {
  const asOperation = (operation: BrewerPatchOperation) => operation;
  const asPatch = (changes: BrewerPatch) => changes;

  it('cannot express a /brewerId, /isDeletable or /rating operation', () => {
    // @ts-expect-error brewerId is not a patchable path
    asOperation({ op: 'replace', path: '/brewerId', value: 'x' });
    // @ts-expect-error isDeletable is not a patchable path
    asOperation({ op: 'replace', path: '/isDeletable', value: true });
    // @ts-expect-error rating is not a patchable path
    asOperation({ op: 'replace', path: '/rating', value: null });
    // @ts-expect-error breweryType is replaced whole, not by sub-path
    asOperation({ op: 'replace', path: '/breweryType/id', value: 'x' });
    // @ts-expect-error only replace is supported
    asOperation({ op: 'remove', path: '/name', value: 'x' });
    expect(asOperation({ op: 'replace', path: '/name', value: 'x' }).path).toBe(
      '/name',
    );
  });

  it('cannot carry brewerId, isDeletable or rating in a BrewerPatch', () => {
    // @ts-expect-error brewerId is not patchable
    asPatch({ brewerId: BREWER_ID });
    // @ts-expect-error isDeletable is not patchable
    asPatch({ isDeletable: false });
    // @ts-expect-error rating is not patchable
    asPatch({ rating: null });
    expect(asPatch({ name: 'x' })).toEqual({ name: 'x' });
  });
});
