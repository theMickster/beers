import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import {
  BeerCompareStore,
  CompareSelectionStore,
  type CompareColumn,
} from '@beers/beer/data-access';
import { API_BASE_URL } from '@beers/shared/data-access';
import { A, B, C, D, E, F, failed, loaded, makeBeer } from '../test-data';
import { BeerCompareComponent } from './beer-compare';

@Component({ template: '' })
class StubComponent {}

const ROUTES = [
  { path: 'compare', component: BeerCompareComponent },
  { path: 'explorer', component: StubComponent },
  { path: 'beers/:beerId', component: StubComponent },
];

function createFakeCompareStore() {
  return {
    ids: signal<readonly string[]>([]),
    columns: signal<readonly CompareColumn[]>([]),
    isFulfilled: signal(true),
    load: vi.fn(),
  };
}

describe('BeerCompareComponent', () => {
  let store: ReturnType<typeof createFakeCompareStore>;
  let selection: InstanceType<typeof CompareSelectionStore>;
  let harness: RouterTestingHarness | undefined;
  let el: HTMLElement;

  /** The first visit creates the harness, so a test can set up the stores before the page exists. */
  async function visit(url: string) {
    harness ??= await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    await harness.fixture.whenStable();
    harness.detectChanges();
    el = harness.routeNativeElement as HTMLElement;
  }

  /** Puts a finished comparison of these columns in the store, as if it had loaded for them. */
  function showColumns(...columns: CompareColumn[]) {
    store.ids.set(columns.map((column) => column.beerId));
    store.columns.set(columns);
  }

  const query = <T extends Element>(selector: string) =>
    el.querySelector<T>(selector);
  const text = (node: Element | null | undefined) =>
    node?.textContent?.replace(/\s+/g, ' ').trim();
  const cells = (testId: string) =>
    [...el.querySelectorAll(`[data-testid="${testId}"]`)].map(text);
  const currentUrl = () => TestBed.inject(Router).url;

  beforeEach(() => {
    harness = undefined;
    store = createFakeCompareStore();
    TestBed.configureTestingModule({
      providers: [
        provideRouter(ROUTES),
        { provide: BeerCompareStore, useValue: store },
      ],
    });
    selection = TestBed.inject(CompareSelectionStore);
  });

  describe('loading the comparison', () => {
    it('asks the store for the ids in the URL, lowercased, once the browser has rendered', async () => {
      await visit(`/compare?beers=${A.toUpperCase()},${B}`);

      expect(store.load).toHaveBeenCalledTimes(1);
      expect(store.load).toHaveBeenCalledWith([A, B]);
    });

    it('loads only the first 4 when the URL carries more', async () => {
      await visit(`/compare?beers=${[A, B, C, D, E, F].join(',')}`);

      expect(store.load).toHaveBeenCalledWith([A, B, C, D]);
    });

    it('loads the next set when the ids change', async () => {
      await visit(`/compare?beers=${A},${B}`);

      await visit(`/compare?beers=${C},${D}`);

      expect(store.load).toHaveBeenCalledTimes(2);
      expect(store.load).toHaveBeenLastCalledWith([C, D]);
    });

    // `state` is a new object on every query emission, so reading it would reload here.
    it('does not load again when only an unrelated query param changes', async () => {
      await visit(`/compare?beers=${A},${B}`);

      await visit(`/compare?beers=${A},${B}&utm=x`);

      expect(store.load).toHaveBeenCalledTimes(1);
    });

    it('does not load again when the same ids come back in another case', async () => {
      await visit(`/compare?beers=${A},${B}`);

      await visit(`/compare?beers=${A.toUpperCase()},${B}`);

      expect(store.load).toHaveBeenCalledTimes(1);
    });

    it.each([
      ['one id', `/compare?beers=${A}`],
      ['the same id twice', `/compare?beers=${A},${A.toUpperCase()}`],
      ['only junk', '/compare?beers=junk,123'],
      ['an empty param', '/compare?beers='],
      ['no param', '/compare'],
    ])('makes no request for %s', async (_, url) => {
      await visit(url);

      expect(store.load).not.toHaveBeenCalled();
    });
  });

  describe('fewer than 2 usable ids', () => {
    it.each([
      ['no param', '/compare'],
      ['one id', `/compare?beers=${A}`],
      ['junk', '/compare?beers=junk'],
    ])('shows the prompt with a way to the explorer for %s', async (_, url) => {
      await visit(url);

      const prompt = query('[data-testid="compare-empty"]');
      expect(text(prompt)).toContain(
        'Pick at least 2 different beers to compare',
      );
      expect(prompt?.querySelector('a')?.getAttribute('href')).toBe(
        '/explorer',
      );
      expect(query('[data-testid="loading"]')).toBeNull();
      expect(query('table')).toBeNull();
    });

    it('keeps one level-1 heading so every state has a page title', async () => {
      await visit('/compare');

      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(text(query('h1'))).toBe('Compare');
    });
  });

  describe('the selection redirect', () => {
    beforeEach(() => {
      selection.toggle(A);
      selection.toggle(B);
    });

    it('turns a bare /compare into the shareable link, replacing the history entry', async () => {
      const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

      await visit('/compare');

      expect(currentUrl()).toBe(`/compare?beers=${A},${B}`);
      expect(navigate).toHaveBeenCalledTimes(1);
      expect(navigate.mock.calls[0][1]).toMatchObject({ replaceUrl: true });
      expect(store.load).toHaveBeenCalledTimes(1);
      expect(store.load).toHaveBeenCalledWith([A, B]);
    });

    it('keeps unrelated query params', async () => {
      await visit('/compare?utm=x');

      expect(currentUrl()).toBe(`/compare?utm=x&beers=${A},${B}`);
    });

    it.each([
      ['junk', 'junk'],
      ['an empty value', ''],
      ['one id', C],
    ])(
      'never replaces a link that has a beers param with %s',
      async (_, value) => {
        const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

        await visit(`/compare?beers=${value}`);

        expect(navigate).not.toHaveBeenCalled();
        expect(currentUrl()).toBe(`/compare?beers=${value}`);
        expect(query('[data-testid="compare-empty"]')).not.toBeNull();
      },
    );

    // The ids are '' in both URLs, so only `hasParam` tells them apart.
    it('redirects when the user moves from an unusable link to a bare /compare', async () => {
      await visit('/compare?beers=junk');

      await visit('/compare');

      expect(currentUrl()).toBe(`/compare?beers=${A},${B}`);
    });

    it('does not redirect when fewer than 2 beers are selected', async () => {
      selection.toggle(B);
      const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

      await visit('/compare');

      expect(navigate).not.toHaveBeenCalled();
      expect(query('[data-testid="compare-empty"]')).not.toBeNull();
    });
  });

  describe('the over-cap notice', () => {
    it('says comparisons are capped when more than 4 unique beers are in the link', async () => {
      showColumns(loaded({ beerId: A }), loaded({ beerId: B }));
      await visit(`/compare?beers=${[A, B, C, D, E].join(',')}`);

      expect(text(query('[data-testid="compare-capped"]'))).toBe(
        'Comparisons are capped at 4 beers. Showing the first 4.',
      );
    });

    it('shows the notice while the comparison is still loading', async () => {
      await visit(`/compare?beers=${[A, B, C, D, E].join(',')}`);

      expect(query('[data-testid="loading"]')).not.toBeNull();
      expect(query('[data-testid="compare-capped"]')).not.toBeNull();
    });

    it.each([
      ['exactly 4 ids', [A, B, C, D].join(',')],
      [
        '4 unique ids with a duplicate',
        [A, A.toUpperCase(), B, C, D].join(','),
      ],
    ])('does not show it for %s', async (_, beers) => {
      await visit(`/compare?beers=${beers}`);

      expect(query('[data-testid="compare-capped"]')).toBeNull();
    });
  });

  describe('which comparison is current', () => {
    it('shows the loading state, not a finished comparison, while the store is pending', async () => {
      showColumns(loaded({ beerId: A }), loaded({ beerId: B }));
      store.isFulfilled.set(false);

      await visit(`/compare?beers=${A},${B}`);

      const loading = query('[data-testid="loading"]');
      expect(loading?.getAttribute('role')).toBe('status');
      expect(loading?.getAttribute('aria-busy')).toBe('true');
      expect(text(loading)).toContain('Loading comparison');
      expect(query('table')).toBeNull();
      expect(el.querySelectorAll('h1')).toHaveLength(1);
    });

    // The store never loads here, so the first frame under the new URL still holds the old ids.
    it('shows loading, none of the previous comparison, when the store holds other ids', async () => {
      showColumns(
        loaded({ beerId: A, name: 'Alpha' }),
        loaded({ beerId: B, name: 'Bravo' }),
      );

      await visit(`/compare?beers=${C},${D}`);

      expect(query('[data-testid="loading"]')).not.toBeNull();
      expect(query('table')).toBeNull();
      expect(el.textContent).not.toContain('Alpha');
      expect(el.textContent).not.toContain('Bravo');
    });

    it('shows loading when the store holds only some of the same ids', async () => {
      showColumns(loaded({ beerId: A, name: 'Alpha' }), loaded({ beerId: B }));

      await visit(`/compare?beers=${A},${C}`);

      expect(query('[data-testid="loading"]')).not.toBeNull();
      expect(el.textContent).not.toContain('Alpha');
    });

    it('shows the comparison when the store holds these ids, even in another case', async () => {
      showColumns(loaded({ beerId: A }), loaded({ beerId: B }));

      await visit(`/compare?beers=${A.toUpperCase()},${B}`);

      expect(query('table')).not.toBeNull();
      expect(query('[data-testid="loading"]')).toBeNull();
    });
  });

  describe('a finished comparison', () => {
    it('has one column per beer, each name linking to its detail page', async () => {
      showColumns(
        loaded({ beerId: A, name: 'Alpha' }),
        loaded({ beerId: B, name: 'Bravo' }),
        loaded({ beerId: C, name: 'Charlie' }),
      );
      await visit(`/compare?beers=${A},${B},${C}`);

      expect(cells('compare-name')).toEqual(['Alpha', 'Bravo', 'Charlie']);
      const links = [...el.querySelectorAll('thead a')].map((a) =>
        a.getAttribute('href'),
      );
      expect(links).toEqual([`/beers/${A}`, `/beers/${B}`, `/beers/${C}`]);
      expect(el.querySelectorAll('h1')).toHaveLength(1);
    });

    it('aligns image, price, rating and styles in rows led by a row header', async () => {
      showColumns(loaded({ beerId: A }), loaded({ beerId: B }));
      await visit(`/compare?beers=${A},${B}`);

      const rows = [...el.querySelectorAll('tbody tr')];
      expect(
        rows.map((row) => text(row.querySelector('th[scope="row"]'))),
      ).toEqual(['Image', 'Price', 'Rating', 'Styles']);
      expect(rows.every((row) => row.querySelectorAll('td').length === 2)).toBe(
        true,
      );
      expect(text(query('caption'))).toContain('2 beers');
    });

    it('shows the image with the beer name as its alternative text', async () => {
      showColumns(loaded({ beerId: A, name: 'Alpha' }), loaded({ beerId: B }));
      await visit(`/compare?beers=${A},${B}`);

      const image = query('tbody img');
      expect(image?.getAttribute('src')).toBe('https://img.test/hazy.png');
      expect(image?.getAttribute('alt')).toBe('Alpha');
    });

    it('shows an icon instead of an image for a beer without one', async () => {
      showColumns(loaded({ beerId: A, image: '' }), loaded({ beerId: B }));
      await visit(`/compare?beers=${A},${B}`);

      expect(el.querySelectorAll('tbody img')).toHaveLength(1);
      expect(el.querySelectorAll('tbody .fa-beer-mug-empty')).toHaveLength(1);
    });

    describe('price', () => {
      it('shows one price when every tier costs the same, a range otherwise', async () => {
        showColumns(
          loaded({
            beerId: A,
            pricing: [
              { price: 9.99, quantity: 6, unitVolume: 12, packaging: 'Can' },
              { price: 9.99, quantity: 12, unitVolume: 12, packaging: 'Can' },
            ],
          }),
          loaded({
            beerId: B,
            pricing: [
              { price: 18, quantity: 12, unitVolume: 16, packaging: 'Bottle' },
              { price: 9.99, quantity: 6, unitVolume: 12, packaging: 'Can' },
            ],
          }),
        );
        await visit(`/compare?beers=${A},${B}`);

        expect(cells('compare-price')).toEqual(['$9.99', '$9.99 – $18.00']);
      });

      it.each([
        ['null', null],
        ['empty', []],
      ])('says price is unavailable when pricing is %s', async (_, pricing) => {
        showColumns(loaded({ beerId: A, pricing }), loaded({ beerId: B }));
        await visit(`/compare?beers=${A},${B}`);

        expect(cells('compare-price')[0]).toBe('Price unavailable');
      });
    });

    describe('rating', () => {
      it('shows each beer rating and says so when there are no reviews', async () => {
        showColumns(
          loaded({ beerId: A, rating: { average: 4.25, reviewCount: 12 } }),
          loaded({ beerId: B, rating: null }),
          loaded({ beerId: C, rating: { average: 0, reviewCount: 0 } }),
        );
        await visit(`/compare?beers=${A},${B},${C}`);

        expect(cells('compare-rating')).toEqual([
          '4.3 (12 reviews)',
          'No reviews yet',
          'No reviews yet',
        ]);
      });
    });

    describe('styles', () => {
      it('joins the style names, or shows a dash when there are none', async () => {
        showColumns(
          loaded({
            beerId: A,
            beerStyles: [
              { id: 's1', name: 'IPA' },
              { id: 's2', name: 'Hazy' },
            ],
          }),
          loaded({ beerId: B, beerStyles: [] }),
        );
        await visit(`/compare?beers=${A},${B}`);

        expect(cells('compare-styles')).toEqual([
          'IPA, Hazy',
          '—Not available',
        ]);
      });
    });

    describe('a beer that could not be loaded', () => {
      it.each([
        [404, 'This beer no longer exists.'],
        [500, 'Could not load this beer.'],
      ])(
        'keeps its column and says why for status %i, with dashes below',
        async (status, message) => {
          showColumns(loaded({ beerId: A, name: 'Alpha' }), failed(B, status));
          await visit(`/compare?beers=${A},${B}`);

          expect(cells('compare-name')).toEqual(['Alpha', message]);
          expect(cells('compare-price')[1]).toBe('—Not available');
          expect(cells('compare-rating')[1]).toBe('—Not available');
          expect(cells('compare-styles')[1]).toBe('—Not available');
          const rows = [...el.querySelectorAll('tbody tr')];
          expect(rows.every((r) => r.querySelectorAll('td').length === 2)).toBe(
            true,
          );
          expect(query('[role="alert"]')).toBeNull();
          expect(query('tbody tr td img')?.getAttribute('alt')).toBe('Alpha');
        },
      );
    });
  });
});

describe('BeerCompareComponent with the real BeerCompareStore', () => {
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

  const beerRequest = (id: string) => http.expectOne(`${BASE_URL}/beers/${id}`);
  const names = (el: HTMLElement) =>
    [...el.querySelectorAll('[data-testid="compare-name"]')].map((th) =>
      th.textContent?.trim(),
    );

  it('shows loading, never the first set, while the second set loads', async () => {
    const first = await visit(`/compare?beers=${A},${B}`);
    beerRequest(A).flush(makeBeer({ beerId: A, name: 'Alpha' }));
    beerRequest(B).flush(makeBeer({ beerId: B, name: 'Bravo' }));
    harness.detectChanges();
    expect(names(first)).toEqual(['Alpha', 'Bravo']);

    const page = await visit(`/compare?beers=${C},${D}`);

    const pending = [beerRequest(C), beerRequest(D)];
    expect(page.textContent).not.toContain('Alpha');
    expect(page.querySelector('[data-testid="loading"]')).not.toBeNull();

    pending[0].flush(makeBeer({ beerId: C, name: 'Charlie' }));
    pending[1].flush(makeBeer({ beerId: D, name: 'Delta' }));
    harness.detectChanges();
    expect(names(page)).toEqual(['Charlie', 'Delta']);
  });

  it('shows loading, not the previous set, when the user leaves and returns with other ids', async () => {
    await visit(`/compare?beers=${A},${B}`);
    beerRequest(A).flush(makeBeer({ beerId: A, name: 'Alpha' }));
    beerRequest(B).flush(makeBeer({ beerId: B, name: 'Bravo' }));
    await visit('/explorer');

    const page = await visit(`/compare?beers=${C},${D}`);

    const pending = [beerRequest(C), beerRequest(D)];
    expect(page.textContent).not.toContain('Alpha');
    expect(page.querySelector('[data-testid="loading"]')).not.toBeNull();
    pending.forEach((request) => request.flush(makeBeer()));
  });

  it('turns the selection into a shareable link and loads exactly those beers', async () => {
    const selection = TestBed.inject(CompareSelectionStore);
    selection.toggle(A);
    selection.toggle(B);

    const page = await visit('/compare');

    expect(TestBed.inject(Router).url).toBe(`/compare?beers=${A},${B}`);
    beerRequest(A).flush(makeBeer({ beerId: A, name: 'Alpha' }));
    beerRequest(B).flush(makeBeer({ beerId: B, name: 'Bravo' }));
    harness.detectChanges();
    expect(names(page)).toEqual(['Alpha', 'Bravo']);
  });

  it('keeps a deleted beer as an unavailable column next to the others', async () => {
    const page = await visit(`/compare?beers=${A},${B}`);
    beerRequest(A).flush(makeBeer({ beerId: A, name: 'Alpha' }));
    beerRequest(B).flush(['Beer not found.'], {
      status: 404,
      statusText: 'Not Found',
    });
    harness.detectChanges();

    expect(names(page)).toEqual(['Alpha', 'This beer no longer exists.']);
  });
});
