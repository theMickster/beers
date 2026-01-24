import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { BeerStore, type Beer } from '@beers/beer/data-access';
import { MetadataStore } from '@beers/metadata/data-access';
import { ApiError } from '@beers/shared/data-access';
import { GUID_A, GUID_B, makeBeer } from '../test-data';
import { ExplorerComponent } from './explorer';

function createFakeBeerStore() {
  return {
    results: signal<Beer[]>([]),
    pageNumber: signal(1),
    totalPages: signal(0),
    totalRecords: signal(0),
    hasPreviousPage: signal(false),
    hasNextPage: signal(false),
    isIdle: signal(false),
    isPending: signal(false),
    isFulfilled: signal(true),
    error: signal<ApiError | null>(null),
    search: vi.fn(),
  };
}

function createFakeMetadataStore() {
  return {
    beerStyles: signal([
      { id: GUID_A, name: 'IPA' },
      { id: GUID_B, name: 'Stout' },
    ]),
    beerCategories: signal([{ id: GUID_B, name: 'Domestic' }]),
    beerTypes: signal([{ id: GUID_A, name: 'Ale' }]),
    error: signal<ApiError | null>(null),
    load: vi.fn(() => Promise.resolve()),
  };
}

describe('ExplorerComponent', () => {
  let beers: ReturnType<typeof createFakeBeerStore>;
  let metadata: ReturnType<typeof createFakeMetadataStore>;
  let harness: RouterTestingHarness;
  let el: HTMLElement;

  async function open(url: string) {
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url, ExplorerComponent);
    await harness.fixture.whenStable();
    harness.detectChanges();
    el = harness.routeNativeElement as HTMLElement;
  }

  async function settle() {
    await harness.fixture.whenStable();
    harness.detectChanges();
  }

  const lastSearch = () => beers.search.mock.lastCall?.[0];
  const currentUrl = () => TestBed.inject(Router).url;
  const query = <T extends Element>(selector: string) =>
    el.querySelector<T>(selector);
  const checkboxFor = (label: string) =>
    [...el.querySelectorAll('label')]
      .find((l) => l.textContent?.trim() === label)
      ?.querySelector<HTMLInputElement>('input') ?? null;
  const button = (name: string) =>
    [...el.querySelectorAll('button')].find(
      (b) => b.textContent?.trim() === name,
    );
  const type = (id: string, value: string) => {
    const input = query<HTMLInputElement>(`#${id}`);
    if (!input) throw new Error(`Missing #${id}`);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    harness.detectChanges();
  };

  beforeEach(() => {
    beers = createFakeBeerStore();
    metadata = createFakeMetadataStore();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'explorer', component: ExplorerComponent }]),
        { provide: BeerStore, useValue: beers },
        { provide: MetadataStore, useValue: metadata },
      ],
    });
  });

  describe('URL to store', () => {
    it('searches with defaults for a bare /explorer', async () => {
      await open('/explorer');

      expect(metadata.load).toHaveBeenCalledTimes(1);
      expect(beers.search).toHaveBeenCalledTimes(1);
      expect(lastSearch()).toEqual({
        page: { pageNumber: 1, pageSize: 12 },
        filters: {
          beerStyleIds: [],
          beerCategoryIds: [],
          beerTypeIds: [],
          minPrice: undefined,
          maxPrice: undefined,
        },
      });
    });

    it('turns every query param into the matching search argument', async () => {
      await open(
        `/explorer?style=${GUID_A}&style=${GUID_B}&category=${GUID_B}&type=${GUID_A}&minPrice=5&maxPrice=15.5&page=3&pageSize=24`,
      );

      expect(lastSearch()).toEqual({
        page: { pageNumber: 3, pageSize: 24 },
        filters: {
          beerStyleIds: [GUID_A, GUID_B],
          beerCategoryIds: [GUID_B],
          beerTypeIds: [GUID_A],
          minPrice: 5,
          maxPrice: 15.5,
        },
      });
    });

    it('checks the options and fills the price inputs from the URL', async () => {
      await open(`/explorer?style=${GUID_B}&minPrice=5&maxPrice=9`);

      expect(checkboxFor('IPA')?.checked).toBe(false);
      expect(checkboxFor('Stout')?.checked).toBe(true);
      expect(query<HTMLInputElement>('#explorer-min-price')?.value).toBe('5');
      expect(query<HTMLInputElement>('#explorer-max-price')?.value).toBe('9');
    });

    it('drops invalid params instead of sending them to the API', async () => {
      await open(
        `/explorer?style=not-a-guid&style=${GUID_A}&type=zzz&minPrice=-4&maxPrice=abc&page=0&pageSize=500`,
      );

      expect(lastSearch()).toEqual({
        page: { pageNumber: 1, pageSize: 12 },
        filters: {
          beerStyleIds: [GUID_A],
          beerCategoryIds: [],
          beerTypeIds: [],
          minPrice: undefined,
          maxPrice: undefined,
        },
      });
    });

    it('searches again when the URL changes', async () => {
      await open('/explorer');

      await harness.navigateByUrl('/explorer?page=2', ExplorerComponent);
      await settle();

      expect(beers.search).toHaveBeenCalledTimes(2);
      expect(lastSearch().page.pageNumber).toBe(2);
    });
  });

  describe('filter changes to URL', () => {
    it('adds a style to the URL and resets the page', async () => {
      await open('/explorer?page=4');

      checkboxFor('IPA')?.click();
      await settle();

      expect(currentUrl()).toBe(`/explorer?style=${GUID_A}`);
      expect(lastSearch().filters.beerStyleIds).toEqual([GUID_A]);
      expect(lastSearch().page.pageNumber).toBe(1);
    });

    it('repeats the param for several selections and removes it when unchecked', async () => {
      await open(`/explorer?style=${GUID_A}`);

      checkboxFor('Stout')?.click();
      await settle();
      expect(currentUrl()).toBe(`/explorer?style=${GUID_A}&style=${GUID_B}`);

      checkboxFor('IPA')?.click();
      await settle();
      expect(currentUrl()).toBe(`/explorer?style=${GUID_B}`);

      checkboxFor('Stout')?.click();
      await settle();
      expect(currentUrl()).toBe('/explorer');
    });

    it('keeps other filters when one changes', async () => {
      await open(`/explorer?type=${GUID_A}&minPrice=3`);

      checkboxFor('Domestic')?.click();
      await settle();

      expect(currentUrl()).toBe(
        `/explorer?type=${GUID_A}&minPrice=3&category=${GUID_B}`,
      );
    });

    it('applies the price range to the URL', async () => {
      await open('/explorer');

      type('explorer-min-price', '5');
      type('explorer-max-price', '12');
      button('Apply price')?.click();
      await settle();

      expect(currentUrl()).toBe('/explorer?minPrice=5&maxPrice=12');
      expect(lastSearch().filters).toMatchObject({ minPrice: 5, maxPrice: 12 });
    });

    it('removes a price bound that is cleared', async () => {
      await open('/explorer?minPrice=5&maxPrice=12');

      type('explorer-max-price', '');
      button('Apply price')?.click();
      await settle();

      expect(currentUrl()).toBe('/explorer?minPrice=5');
    });

    it('stores a non-default page size and drops the default one', async () => {
      await open('/explorer?page=3');
      const select = query<HTMLSelectElement>('select');
      if (!select) throw new Error('Missing page size select');

      select.value = '48';
      select.dispatchEvent(new Event('change'));
      await settle();
      expect(currentUrl()).toBe('/explorer?pageSize=48');

      select.value = '12';
      select.dispatchEvent(new Event('change'));
      await settle();
      expect(currentUrl()).toBe('/explorer');
    });

    it('clears every filter but keeps the page size', async () => {
      await open(`/explorer?style=${GUID_A}&minPrice=2&pageSize=24`);

      button('Clear filters')?.click();
      await settle();

      expect(currentUrl()).toBe('/explorer?pageSize=24');
    });
  });

  describe('price validation', () => {
    it('blocks a range where min is greater than max and explains why', async () => {
      await open('/explorer');
      const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

      type('explorer-min-price', '20');
      type('explorer-max-price', '5');

      expect(query('#explorer-price-error')?.textContent).toContain(
        'Minimum price cannot be greater than maximum price',
      );
      expect(button('Apply price')?.disabled).toBe(true);

      query<HTMLFormElement>('form')?.dispatchEvent(new Event('submit'));
      await settle();

      expect(navigate).not.toHaveBeenCalled();
      expect(currentUrl()).toBe('/explorer');
    });

    it('accepts equal bounds and clears the message once fixed', async () => {
      await open('/explorer');

      type('explorer-min-price', '20');
      type('explorer-max-price', '5');
      type('explorer-max-price', '20');

      expect(query('#explorer-price-error')).toBeNull();
      expect(button('Apply price')?.disabled).toBe(false);
    });

    it('rejects a negative price', async () => {
      await open('/explorer');

      type('explorer-min-price', '-1');

      expect(query('#explorer-price-error')?.textContent).toContain(
        'numbers of 0 or more',
      );
      expect(button('Apply price')?.disabled).toBe(true);
    });
  });

  describe('results and paging', () => {
    it('renders a card per beer and the total', async () => {
      beers.results.set([
        makeBeer({ beerId: 'b1', name: 'One' }),
        makeBeer({ beerId: 'b2', name: 'Two' }),
      ]);
      beers.totalRecords.set(2);
      await open('/explorer');

      expect(el.querySelectorAll('bw-beer-card')).toHaveLength(2);
      expect(query('[data-testid="result-count"]')?.textContent?.trim()).toBe(
        '2 beers',
      );
    });

    it('shows loading placeholders while idle or first loading', async () => {
      beers.isIdle.set(true);
      beers.isFulfilled.set(false);
      await open('/explorer');

      expect(query('[data-testid="loading"]')).toBeTruthy();
      expect(query('bw-empty-state')).toBeNull();
    });

    it('builds page controls from the store and navigates', async () => {
      beers.results.set([makeBeer()]);
      beers.pageNumber.set(2);
      beers.totalPages.set(5);
      beers.totalRecords.set(55);
      beers.hasPreviousPage.set(true);
      beers.hasNextPage.set(true);
      await open('/explorer?page=2');

      expect(
        query(
          'nav[aria-label="Pagination"] [aria-current="page"]',
        )?.textContent?.trim(),
      ).toBe('2');

      button('Next')?.click();
      await settle();
      expect(currentUrl()).toBe('/explorer?page=3');

      button('Previous')?.click();
      await settle();
      // The fake store's page number is still 2, so Previous goes to page 1, which is the default.
      expect(currentUrl()).toBe('/explorer');
    });

    it('disables Previous on the first page and Next on the last', async () => {
      beers.results.set([makeBeer()]);
      beers.totalPages.set(2);
      beers.hasNextPage.set(true);
      await open('/explorer');
      expect(button('Previous')?.disabled).toBe(true);
      expect(button('Next')?.disabled).toBe(false);

      beers.pageNumber.set(2);
      beers.hasPreviousPage.set(true);
      beers.hasNextPage.set(false);
      harness.detectChanges();
      expect(button('Previous')?.disabled).toBe(false);
      expect(button('Next')?.disabled).toBe(true);
    });

    it('hides the pager when there is a single page', async () => {
      beers.results.set([makeBeer()]);
      beers.totalPages.set(1);
      await open('/explorer');

      expect(query('nav[aria-label="Pagination"]')).toBeNull();
    });
  });

  describe('empty and error states', () => {
    it('shows the shared empty state when nothing matches', async () => {
      await open(`/explorer?style=${GUID_A}`);

      const empty = query('bw-empty-state');
      expect(empty?.textContent).toContain('No beers match');
      expect(query('[data-testid="beer-grid"]')).toBeNull();
      expect(button('Clear filters')).toBeTruthy();
    });

    it('shows the error and retries the same search', async () => {
      beers.isFulfilled.set(false);
      beers.error.set(new ApiError(500, ['Boom.'], 'server'));
      await open('/explorer?page=2');

      expect(query('[role="alert"].alert-error')?.textContent).toContain(
        'Boom.',
      );
      expect(query('bw-empty-state')).toBeNull();

      beers.search.mockClear();
      button('Try again')?.click();

      expect(beers.search).toHaveBeenCalledTimes(1);
      expect(lastSearch().page.pageNumber).toBe(2);
    });

    it('warns when the filter options could not be loaded', async () => {
      metadata.error.set(new ApiError(0, ['offline'], 'network'));
      await open('/explorer');

      expect(el.textContent).toContain('Filter options could not be loaded');
    });
  });
});
