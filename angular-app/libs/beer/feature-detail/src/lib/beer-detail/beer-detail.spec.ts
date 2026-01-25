import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import {
  BeerDetailStore,
  CompareSelectionStore,
  MAX_COMPARE_BEERS,
  type Beer,
} from '@beers/beer/data-access';
import { API_BASE_URL, ApiError } from '@beers/shared/data-access';
import { GUID_A, GUID_B, makeBeer } from '../test-data';
import { BeerDetailComponent } from './beer-detail';

function createFakeDetailStore() {
  return {
    beer: signal<Beer | null>(null),
    error: signal<ApiError | null>(null),
    notFound: signal(false),
    load: vi.fn(),
  };
}

describe('BeerDetailComponent', () => {
  let detail: ReturnType<typeof createFakeDetailStore>;
  let compare: InstanceType<typeof CompareSelectionStore>;
  let harness: RouterTestingHarness;
  let el: HTMLElement;

  async function open(id: string = GUID_A) {
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(`/beers/${id}`, BeerDetailComponent);
    await settle();
    el = harness.routeNativeElement as HTMLElement;
  }

  async function settle() {
    await harness.fixture.whenStable();
    harness.detectChanges();
  }

  const query = <T extends Element>(selector: string) =>
    el.querySelector<T>(selector);
  const textOf = (selector: string) =>
    query(selector)?.textContent?.replace(/\s+/g, ' ').trim();
  const button = (name: string) =>
    [...el.querySelectorAll('button')].find(
      (b) => b.textContent?.trim() === name,
    );
  const compareButton = () =>
    query<HTMLButtonElement>('[data-testid="compare-toggle"]');
  const pricingRows = () =>
    [...el.querySelectorAll('[data-testid="pricing-table"] tbody tr')].map(
      (row) =>
        [...row.querySelectorAll('td')].map((cell) => cell.textContent?.trim()),
    );

  async function showBeer(overrides: Partial<Beer> = {}) {
    detail.beer.set(makeBeer(overrides));
    await open();
  }

  beforeEach(() => {
    detail = createFakeDetailStore();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'beers/:beerId', component: BeerDetailComponent },
        ]),
        { provide: BeerDetailStore, useValue: detail },
      ],
    });
    compare = TestBed.inject(CompareSelectionStore);
  });

  describe('loading the beer', () => {
    it('asks the store for the beer in the route once the browser has rendered', async () => {
      await open(GUID_A);

      expect(detail.load).toHaveBeenCalledTimes(1);
      expect(detail.load).toHaveBeenCalledWith(GUID_A);
    });

    it('loads the next beer when the route changes to another id', async () => {
      await open(GUID_A);

      await harness.navigateByUrl(`/beers/${GUID_B}`);
      await settle();

      expect(detail.load).toHaveBeenCalledTimes(2);
      expect(detail.load).toHaveBeenLastCalledWith(GUID_B);
    });

    it('shows the loading state while there is no beer, error or not-found', async () => {
      await open();

      const loading = query('[data-testid="loading"]');
      expect(loading?.getAttribute('role')).toBe('status');
      expect(loading?.getAttribute('aria-busy')).toBe('true');
      expect(loading?.textContent).toContain('Loading beer');
      expect(query('h1')).toBeNull();
      expect(query('[role="alert"]')).toBeNull();
    });
  });

  describe('a loaded beer', () => {
    it('shows the name, brewer link, description and image', async () => {
      await showBeer();

      expect(textOf('h1')).toBe('Hazy Day');
      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(textOf('[data-testid="brewer-link"]')).toBe('Odell');
      expect(query('[data-testid="brewer-link"]')?.getAttribute('href')).toBe(
        `/brewers/${GUID_B}`,
      );
      expect(el.textContent).toContain('A juicy, unfiltered IPA.');
      expect(query('img')?.getAttribute('src')).toBe(
        'https://img.test/hazy.png',
      );
      expect(query('img')?.getAttribute('alt')).toBe('Hazy Day');
      expect(query('[data-testid="loading"]')).toBeNull();
    });

    it('shows a placeholder icon instead of an image when the beer has none', async () => {
      await showBeer({ image: '' });

      expect(query('img')).toBeNull();
      expect(query('.fa-beer-mug-empty')).not.toBeNull();
    });

    it('says so when the beer has no description', async () => {
      await showBeer({ description: '' });

      expect(el.textContent).toContain('No description available.');
    });

    it('links back to the explorer', async () => {
      await showBeer();

      const back = [...el.querySelectorAll('a')].find((a) =>
        a.textContent?.includes('Back to Explorer'),
      );
      expect(back?.getAttribute('href')).toBe('/explorer');
    });

    describe('rating', () => {
      it('shows the average and a plural review count', async () => {
        await showBeer();

        expect(textOf('[data-testid="beer-rating"]')).toBe('4.3 (12 reviews)');
      });

      it('uses the singular for one review', async () => {
        await showBeer({ rating: { average: 5, reviewCount: 1 } });

        expect(textOf('[data-testid="beer-rating"]')).toBe('5.0 (1 review)');
      });

      it.each([
        ['has no rating', null],
        ['has a rating with no reviews', { average: 0, reviewCount: 0 }],
      ])('says there are no reviews when the beer %s', async (_, rating) => {
        await showBeer({ rating });

        expect(textOf('[data-testid="beer-rating"]')).toBe('No reviews yet');
      });
    });

    describe('pricing', () => {
      it('lists every tier with packaging, quantity, unit volume and price', async () => {
        await showBeer();

        expect(pricingRows()).toEqual([
          ['Can', '6', '12', '$9.99'],
          ['Bottle', '12', '16', '$18.00'],
        ]);
        const headers = [...el.querySelectorAll('th[scope="col"]')].map((th) =>
          th.textContent?.trim(),
        );
        expect(headers).toEqual([
          'Packaging',
          'Quantity',
          'Unit volume',
          'Price',
        ]);
        expect(query('caption')?.textContent).toContain('Hazy Day');
      });

      it.each([
        ['null', null],
        ['empty', []],
      ])('says pricing is unavailable when it is %s', async (_, pricing) => {
        await showBeer({ pricing });

        expect(query('[data-testid="pricing-table"]')).toBeNull();
        expect(el.textContent).toContain('Pricing unavailable');
      });
    });
  });

  describe('a beer that does not exist', () => {
    beforeEach(async () => {
      detail.notFound.set(true);
      detail.error.set(new ApiError(404, ['Beer not found.'], 'client'));
      await open();
    });

    it('shows the not-found state with a way back, and no error alert', () => {
      const notFound = query('[data-testid="not-found"]');
      expect(notFound?.textContent).toContain('Beer not found');
      expect(notFound?.querySelector('a')?.getAttribute('href')).toBe(
        '/explorer',
      );
      expect(query('[role="alert"]')).toBeNull();
      expect(query('[data-testid="loading"]')).toBeNull();
    });

    it('keeps a level-1 heading so the empty state is not the first heading', () => {
      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(textOf('h1')).toBe('Beer');
    });
  });

  describe('a failed load', () => {
    beforeEach(async () => {
      detail.error.set(
        new ApiError(500, ['The server encountered an error.'], 'server'),
      );
      await open();
    });

    it('shows the message in an alert instead of the beer', () => {
      expect(textOf('[role="alert"]')).toContain(
        'The server encountered an error.',
      );
      expect(query('[data-testid="not-found"]')).toBeNull();
    });

    it('keeps a level-1 heading above the alert', () => {
      expect(el.querySelectorAll('h1')).toHaveLength(1);
      expect(textOf('h1')).toBe('Beer');
    });

    it('tries again for the same beer when asked', async () => {
      button('Try again')?.click();
      await settle();

      expect(detail.load).toHaveBeenCalledTimes(2);
      expect(detail.load).toHaveBeenLastCalledWith(GUID_A);
    });
  });

  describe('Add to Compare', () => {
    const otherBeers = Array.from(
      { length: MAX_COMPARE_BEERS },
      (_, i) => `3f2504e0-4f89-11d3-9a0c-0305e82c340${i}`,
    );

    it('adds the beer on click and offers to remove it, then reverses', async () => {
      await showBeer();
      expect(compareButton()?.textContent?.trim()).toBe('Add to Compare');

      compareButton()?.click();
      await settle();

      expect(compare.isSelected(GUID_A)).toBe(true);
      expect(compareButton()?.textContent?.trim()).toBe('Remove from Compare');

      compareButton()?.click();
      await settle();

      expect(compare.isSelected(GUID_A)).toBe(false);
      expect(compareButton()?.textContent?.trim()).toBe('Add to Compare');
    });

    it('shows a beer already in the selection as removable', async () => {
      compare.toggle(GUID_A);
      await showBeer();

      expect(compareButton()?.textContent?.trim()).toBe('Remove from Compare');
    });

    describe('when the selection is full', () => {
      beforeEach(() => {
        otherBeers.forEach((id) => compare.toggle(id));
      });

      it('disables the button for accessibility tools but keeps it focusable, and explains why', async () => {
        await showBeer();

        const toggle = compareButton();
        expect(toggle?.getAttribute('aria-disabled')).toBe('true');
        expect(toggle?.disabled).toBe(false);
        const helpId = toggle?.getAttribute('aria-describedby');
        expect(helpId).toBeTruthy();
        expect(document.getElementById(helpId ?? '')?.textContent).toContain(
          `up to ${MAX_COMPARE_BEERS} beers`,
        );
      });

      it('does not add the beer when clicked', async () => {
        await showBeer();

        compareButton()?.click();
        await settle();

        expect(compare.isSelected(GUID_A)).toBe(false);
        expect(compare.beerIds()).toEqual(otherBeers);
        expect(compareButton()?.textContent?.trim()).toBe('Add to Compare');
      });

      it('still lets the user remove this beer when it is one of the selected', async () => {
        compare.toggle(otherBeers[0]);
        compare.toggle(GUID_A);
        await showBeer();

        const toggle = compareButton();
        expect(toggle?.getAttribute('aria-disabled')).toBeNull();
        expect(toggle?.textContent?.trim()).toBe('Remove from Compare');
        expect(query('#compare-help')).toBeNull();

        toggle?.click();
        await settle();

        expect(compare.isSelected(GUID_A)).toBe(false);
      });
    });
  });
});

@Component({ template: '' })
class ExplorerStubComponent {}

// The store is root-scoped and outlives the component, so the next beer's page starts from the
// previous beer's state. The fake store above cannot show whether that state ever reaches the DOM.
describe('BeerDetailComponent with the real BeerDetailStore', () => {
  const BASE_URL = 'https://api.test/api/v1';
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
        provideRouter([
          { path: 'beers/:beerId', component: BeerDetailComponent },
          { path: 'explorer', component: ExplorerStubComponent },
        ]),
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

  it('shows the loading state, not the previous beer, while the next beer loads', async () => {
    const first = await visit(`/beers/${GUID_A}`);
    beerRequest(GUID_A).flush(makeBeer({ beerId: GUID_A, name: 'Alpha' }));
    harness.detectChanges();
    expect(first.querySelector('h1')?.textContent).toBe('Alpha');
    await visit('/explorer');

    const page = await visit(`/beers/${GUID_B}`);

    const pending = beerRequest(GUID_B);
    expect(page.textContent).not.toContain('Alpha');
    expect(page.querySelector('[data-testid="loading"]')).not.toBeNull();

    pending.flush(makeBeer({ beerId: GUID_B, name: 'Bravo' }));
    harness.detectChanges();
    expect(page.querySelector('h1')?.textContent).toBe('Bravo');
  });

  it("shows the loading state, not the previous beer's not-found, while the next beer loads", async () => {
    const first = await visit(`/beers/${GUID_A}`);
    beerRequest(GUID_A).flush(['Beer not found.'], {
      status: 404,
      statusText: 'Not Found',
    });
    harness.detectChanges();
    expect(first.querySelector('[data-testid="not-found"]')).not.toBeNull();
    await visit('/explorer');

    const page = await visit(`/beers/${GUID_B}`);

    const pending = beerRequest(GUID_B);
    expect(page.querySelector('[data-testid="not-found"]')).toBeNull();
    expect(page.querySelector('[data-testid="loading"]')).not.toBeNull();
    pending.flush(makeBeer({ beerId: GUID_B, name: 'Bravo' }));
  });
});
