import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '@beers/shared/data-access';
import { firstValueFrom } from 'rxjs';
import { BeerApiService } from './beer-api.service';
import type { Beer, CreateBeer, UpdateBeer } from './models';

const BASE_URL = 'https://api.test/api/v1';

const createBeer: CreateBeer = {
  brewerId: 'br-1',
  name: 'Pils',
  description: 'Crisp',
  image: 'pils.png',
  sku: 'SKU-1',
  isDeletable: true,
  brewer: { id: 'br-1', name: 'Brewer', website: 'https://b.test' },
  beerTypeId: 'type-1',
  beerCategories: ['cat-1'],
  beerStyles: ['style-1'],
};

describe('BeerApiService', () => {
  let httpMock: HttpTestingController;
  let api: BeerApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    api = TestBed.inject(BeerApiService);
  });

  afterEach(() => httpMock.verify());

  describe('search', () => {
    it('sends only the provided page params and an empty body without filters', () => {
      api.search({ pageNumber: 2 }, {}).subscribe();

      const request = httpMock.expectOne(
        (r) => r.url === `${BASE_URL}/beers/search`,
      );
      expect(request.request.method).toBe('POST');
      expect(request.request.params.keys()).toEqual(['PageNumber']);
      expect(request.request.params.get('PageNumber')).toBe('2');
      expect(request.request.body).toEqual({});
      request.flush({});
    });

    it('sends no query string when no page params are provided', () => {
      api.search({}, {}).subscribe();

      const request = httpMock.expectOne(`${BASE_URL}/beers/search`);
      expect(request.request.params.keys()).toEqual([]);
      request.flush({});
    });

    it('sends all page params when provided', () => {
      api
        .search({ pageNumber: 3, pageSize: 20, sortOrder: 'desc' }, {})
        .subscribe();

      const request = httpMock.expectOne(
        (r) => r.url === `${BASE_URL}/beers/search`,
      );
      expect(request.request.params.get('PageNumber')).toBe('3');
      expect(request.request.params.get('PageSize')).toBe('20');
      expect(request.request.params.get('SortOrder')).toBe('desc');
      request.flush({});
    });

    it('includes only the filters that are set', () => {
      api.search({}, { name: 'ipa', brewerName: 'Acme' }).subscribe();

      const request = httpMock.expectOne(`${BASE_URL}/beers/search`);
      expect(request.request.body).toEqual({ name: 'ipa', brewerName: 'Acme' });
      request.flush({});
    });

    it('omits blank filter values', () => {
      api.search({}, { id: '', name: '   ', brewerId: 'br-1' }).subscribe();

      const request = httpMock.expectOne(`${BASE_URL}/beers/search`);
      expect(request.request.body).toEqual({ brewerId: 'br-1' });
      request.flush({});
    });
  });

  describe('list', () => {
    it('GETs /beers', async () => {
      const result = firstValueFrom(api.list());
      const request = httpMock.expectOne(`${BASE_URL}/beers`);
      expect(request.request.method).toBe('GET');
      request.flush([{ beerId: 'b1' }]);
      expect(await result).toHaveLength(1);
    });

    it('maps a 404 to an empty list', async () => {
      const result = firstValueFrom(api.list());
      httpMock
        .expectOne(`${BASE_URL}/beers`)
        .flush('No beers', { status: 404, statusText: 'Not Found' });
      expect(await result).toEqual([]);
    });

    it('propagates other errors', async () => {
      const result = firstValueFrom(api.list());
      httpMock
        .expectOne(`${BASE_URL}/beers`)
        .flush('boom', { status: 500, statusText: 'Server Error' });
      await expect(result).rejects.toMatchObject({ status: 500 });
    });
  });

  it('getById GETs /beers/{id} and encodes the id', () => {
    api.getById('a/b c').subscribe();
    const request = httpMock.expectOne(`${BASE_URL}/beers/a%2Fb%20c`);
    expect(request.request.method).toBe('GET');
    request.flush({});
  });

  it('create POSTs the payload to /beers', () => {
    api.create(createBeer).subscribe();
    const request = httpMock.expectOne(`${BASE_URL}/beers`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(createBeer);
    request.flush({} as Beer);
  });

  it('update PUTs to a URL built from beer.beerId', () => {
    const update: UpdateBeer = { ...createBeer, beerId: 'beer-9' };
    api.update(update).subscribe();
    const request = httpMock.expectOne(`${BASE_URL}/beers/beer-9`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(update);
    request.flush({});
  });

  it('delete DELETEs /beers/{id}', () => {
    api.delete('beer-9').subscribe();
    const request = httpMock.expectOne(`${BASE_URL}/beers/beer-9`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
