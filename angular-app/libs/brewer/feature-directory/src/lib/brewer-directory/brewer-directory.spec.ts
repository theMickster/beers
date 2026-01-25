import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { BrewerStore, type Brewer } from '@beers/brewer/data-access';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import { guid, makeBrewer, makeBrewers, MICRO, REGIONAL } from '../test-data';
import { BrewerDirectoryComponent } from './brewer-directory';

@Component({ template: '' })
class StubComponent {}

const ROUTES = [
  { path: 'brewers', component: BrewerDirectoryComponent },
  { path: 'brewers/:brewerId', component: StubComponent },
];

function createFakeBrewerStore() {
  return {
    entities: signal<readonly Brewer[]>([]),
    error: signal<ApiError | null>(null),
    isFulfilled: signal(true),
    loadAll: vi.fn(),
  };
}

describe('BrewerDirectoryComponent', () => {
  let store: ReturnType<typeof createFakeBrewerStore>;
  let harness: RouterTestingHarness | undefined;
  let el: HTMLElement;

  /** The first visit creates the harness, so a test can set up the store before the page exists. */
  async function visit(url = '/brewers') {
    harness ??= await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    await settle();
    el = harness.routeNativeElement as HTMLElement;
  }

  async function settle() {
    if (!harness) throw new Error('visit a page first');
    await harness.fixture.whenStable();
    harness.detectChanges();
  }

  const query = <T extends Element>(selector: string) =>
    el.querySelector<T>(selector);
  const text = (node: Element | null | undefined) =>
    node?.textContent?.replace(/\s+/g, ' ').trim();
  const names = () =>
    [...el.querySelectorAll('[data-testid="brewer-card"] h2')].map(text);
  const currentUrl = () => TestBed.inject(Router).url;
  const typeSelect = () => query<HTMLSelectElement>('#brewery-type');
  const button = (name: string) =>
    [...el.querySelectorAll('button')].find(
      (b) => b.textContent?.trim() === name,
    );

  async function chooseType(value: string) {
    const select = typeSelect();
    if (!select) throw new Error('no type select');
    select.value = value;
    select.dispatchEvent(new Event('change'));
    await settle();
  }

  beforeEach(() => {
    harness = undefined;
    store = createFakeBrewerStore();
    TestBed.configureTestingModule({
      providers: [
        provideRouter(ROUTES),
        { provide: BrewerStore, useValue: store },
      ],
    });
  });

  describe('loading the brewers', () => {
    it('asks the store once the browser has rendered, and once only', async () => {
      await visit();

      expect(store.loadAll).toHaveBeenCalledTimes(1);
    });

    it('does not load again when the filter or page changes', async () => {
      store.entities.set(makeBrewers(10));
      await visit();

      await visit('/brewers?page=2');
      await visit(`/brewers?type=${REGIONAL.id}`);

      expect(store.loadAll).toHaveBeenCalledTimes(1);
    });

    // A store that outlives the component can hold brewers the server never rendered.
    it('shows the skeleton, not brewers the store already holds, until the store is fulfilled', async () => {
      store.entities.set(makeBrewers(3));
      store.isFulfilled.set(false);

      await visit();

      const loading = query('[data-testid="loading"]');
      expect(loading?.getAttribute('role')).toBe('status');
      expect(loading?.getAttribute('aria-busy')).toBe('true');
      expect(text(loading)).toContain('Loading brewers');
      expect(names()).toEqual([]);
      expect(typeSelect()).toBeNull();
    });
  });

  describe('a loaded directory', () => {
    it('shows 6 brewers a page sorted by name, with a pager', async () => {
      store.entities.set(makeBrewers(10).reverse());

      await visit();

      expect(names()).toEqual([
        'Brewer 01',
        'Brewer 02',
        'Brewer 03',
        'Brewer 04',
        'Brewer 05',
        'Brewer 06',
      ]);
      expect(query('nav[aria-label="Pagination"]')).not.toBeNull();
    });

    it('shows each brewer name as a link, with founded year, headquarters, type and rating', async () => {
      store.entities.set([
        makeBrewer({
          brewerId: guid(7),
          name: 'Odell',
          foundedIn: 1989,
          headquarters: 'Fort Collins, CO',
          breweryType: REGIONAL,
          rating: { average: 4.25, reviewCount: 12 },
        }),
      ]);

      await visit();

      const card = query('[data-testid="brewer-card"]');
      expect(text(card?.querySelector('a'))).toBe('Odell');
      expect(card?.querySelector('a')?.getAttribute('href')).toBe(
        `/brewers/${guid(7)}`,
      );
      expect(text(card?.querySelector('[data-testid="brewer-founded"]'))).toBe(
        'Founded 1989',
      );
      expect(
        text(card?.querySelector('[data-testid="brewer-headquarters"]')),
      ).toBe('Fort Collins, CO');
      expect(text(card?.querySelector('[data-testid="brewer-type"]'))).toBe(
        'Regional',
      );
      expect(text(card?.querySelector('[data-testid="brewer-rating"]'))).toBe(
        '4.3 (12 reviews)',
      );
    });

    it('says a brewer with no rating has no reviews', async () => {
      store.entities.set([makeBrewer({ rating: null })]);

      await visit();

      expect(text(query('[data-testid="brewer-rating"]'))).toBe(
        'No reviews yet',
      );
    });

    it('shows no pager for one page of brewers', async () => {
      store.entities.set(makeBrewers(6));

      await visit();

      expect(names()).toHaveLength(6);
      expect(query('nav[aria-label="Pagination"]')).toBeNull();
    });

    it('keeps one level-1 heading', async () => {
      store.entities.set(makeBrewers(3));

      await visit();

      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(text(query('h1'))).toBe('Brewers');
    });
  });

  describe('paging', () => {
    beforeEach(() => store.entities.set(makeBrewers(10)));

    it('shows the rest on page 2', async () => {
      await visit('/brewers?page=2');

      expect(names()).toEqual([
        'Brewer 07',
        'Brewer 08',
        'Brewer 09',
        'Brewer 10',
      ]);
    });

    it.each([
      ['too large', '99', 'Brewer 07'],
      ['not a number', 'abc', 'Brewer 01'],
      ['zero', '0', 'Brewer 01'],
    ])(
      'falls back to a real page when the page is %s',
      async (_, page, first) => {
        await visit(`/brewers?page=${page}`);

        expect(names()[0]).toBe(first);
        expect(currentUrl()).toBe(`/brewers?page=${page}`);
      },
    );

    it('goes to the next page from the pager and puts it in the URL', async () => {
      await visit();

      button('Next')?.click();
      await settle();

      expect(currentUrl()).toBe('/brewers?page=2');
      expect(names()).toHaveLength(4);
      expect(query('[aria-current="page"]')?.getAttribute('aria-label')).toBe(
        'Page 2',
      );
    });

    it('drops the page param when going back to page 1', async () => {
      await visit('/brewers?page=2');

      button('Previous')?.click();
      await settle();

      expect(currentUrl()).toBe('/brewers');
    });
  });

  describe('the brewery type filter', () => {
    beforeEach(() => store.entities.set(makeBrewers(10)));

    it('has a labelled select with All types and only the types some brewer has', async () => {
      await visit();

      const select = typeSelect();
      expect(text(query('label[for="brewery-type"]'))).toBe('Brewery type');
      expect([...(select?.options ?? [])].map((o) => o.text.trim())).toEqual([
        'All types',
        'Micro',
        'Regional',
      ]);
      expect(select?.value).toBe('');
    });

    it('narrows the list, resets the page and puts the type in the URL', async () => {
      await visit('/brewers?page=2');

      await chooseType(REGIONAL.id);

      expect(currentUrl()).toBe(`/brewers?type=${REGIONAL.id}`);
      expect(names()).toEqual(['Brewer 03', 'Brewer 06', 'Brewer 09']);
      expect(typeSelect()?.value).toBe(REGIONAL.id);
      expect(query('nav[aria-label="Pagination"]')).toBeNull();
    });

    it('removes the type from the URL when All types is chosen', async () => {
      await visit(`/brewers?type=${REGIONAL.id}`);

      await chooseType('');

      expect(currentUrl()).toBe('/brewers');
      expect(names()).toHaveLength(6);
    });

    it('keeps an unrelated query param when the type changes', async () => {
      await visit('/brewers?utm=x');

      await chooseType(MICRO.id);

      expect(currentUrl()).toBe(`/brewers?utm=x&type=${MICRO.id}`);
    });

    it('reads a type no brewer has as All types, without rewriting the URL', async () => {
      await visit(`/brewers?type=${guid(99)}`);

      expect(names()).toHaveLength(6);
      expect(typeSelect()?.value).toBe('');
      expect(currentUrl()).toBe(`/brewers?type=${guid(99)}`);
    });
  });

  describe('a failed load', () => {
    beforeEach(() => {
      store.isFulfilled.set(false);
      store.error.set(
        new ApiError(500, ['The server encountered an error.'], 'server'),
      );
    });

    it('shows the message in an alert instead of brewers, under one heading', async () => {
      store.entities.set(makeBrewers(3));

      await visit();

      expect(text(query('[role="alert"]'))).toContain(
        'The server encountered an error.',
      );
      expect(names()).toEqual([]);
      expect(el.querySelectorAll('h1')).toHaveLength(1);
    });

    it('asks the store again when told to try again', async () => {
      await visit();

      button('Try again')?.click();
      await settle();

      expect(store.loadAll).toHaveBeenCalledTimes(2);
    });
  });

  describe('no brewers at all', () => {
    it('says there are none, with no filter or pager', async () => {
      await visit();

      expect(text(query('[data-testid="directory-empty"]'))).toContain(
        'No brewers found',
      );
      expect(typeSelect()).toBeNull();
      expect(el.querySelectorAll('h1')).toHaveLength(1);
    });
  });
});

describe('BrewerDirectoryComponent with the real BrewerStore', () => {
  const BASE_URL = 'https://api.test/api/v1';
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
        provideRouter(ROUTES),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => http.verify());

  async function visit(url: string): Promise<HTMLElement> {
    await harness.navigateByUrl(url);
    await harness.fixture.whenStable();
    harness.detectChanges();
    return harness.routeNativeElement as HTMLElement;
  }

  const listRequest = () => http.expectOne(`${BASE_URL}/brewers`);
  const names = (el: HTMLElement) =>
    [...el.querySelectorAll('[data-testid="brewer-card"] h2')].map((h) =>
      h.textContent?.trim(),
    );

  it('replaces the earlier list with the new one when the directory is visited again', async () => {
    const first = await visit('/brewers');
    expect(first.querySelector('[data-testid="loading"]')).not.toBeNull();
    listRequest().flush([
      makeBrewer({ brewerId: guid(1), name: 'Alpha' }),
      makeBrewer({ brewerId: guid(2), name: 'Bravo' }),
    ]);
    harness.detectChanges();
    expect(names(first)).toEqual(['Alpha', 'Bravo']);
    await visit(`/brewers/${guid(1)}`);

    const page = await visit('/brewers');

    const refetch = listRequest();
    expect(names(page)).toEqual([]);
    expect(page.querySelector('[data-testid="loading"]')).not.toBeNull();
    refetch.flush([
      makeBrewer({ brewerId: guid(2), name: 'Bravo' }),
      makeBrewer({ brewerId: guid(3), name: 'Charlie' }),
    ]);
    harness.detectChanges();
    expect(names(page)).toEqual(['Bravo', 'Charlie']);
  });

  it('shows the error from a failed load and recovers on try again', async () => {
    const page = await visit('/brewers');
    listRequest().flush('boom', { status: 500, statusText: 'Server Error' });
    harness.detectChanges();
    expect(page.querySelector('[role="alert"]')).not.toBeNull();

    [...page.querySelectorAll('button')]
      .find((b) => b.textContent?.trim() === 'Try again')
      ?.click();
    listRequest().flush([makeBrewer({ name: 'Alpha' })]);
    harness.detectChanges();

    expect(page.querySelector('[role="alert"]')).toBeNull();
    expect(names(page)).toEqual(['Alpha']);
  });
});
