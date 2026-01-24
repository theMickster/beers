import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import type { Beer } from '@beers/beer/data-access';
import { makeBeer } from '../test-data';
import { BeerCardComponent } from './beer-card';

function render(beer: Beer): HTMLElement {
  TestBed.configureTestingModule({ providers: [provideRouter([])] });
  const fixture = TestBed.createComponent(BeerCardComponent);
  fixture.componentRef.setInput('beer', beer);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

const text = (el: HTMLElement, testId: string) =>
  el
    .querySelector(`[data-testid="${testId}"]`)
    ?.textContent?.replace(/\s+/g, ' ')
    .trim();

describe('BeerCardComponent', () => {
  it('shows the image, name, brewer, rating and price range', () => {
    const el = render(makeBeer());

    expect(el.querySelector('img')?.getAttribute('src')).toBe(
      'https://img.test/hazy.png',
    );
    expect(el.querySelector('img')?.getAttribute('alt')).toBe('Hazy Day');
    expect(el.querySelector('h3')?.textContent).toContain('Hazy Day');
    expect(el.textContent).toContain('Odell');
    expect(text(el, 'beer-rating')).toBe('4.3 (12 reviews)');
    expect(text(el, 'beer-price')).toBe('$9.99 – $18.00');
  });

  it('links to the beer detail route', () => {
    const el = render(makeBeer({ beerId: 'abc' }));

    expect(el.querySelector('a')?.getAttribute('href')).toBe('/beers/abc');
  });

  it('uses the singular for one review', () => {
    const el = render(makeBeer({ rating: { average: 5, reviewCount: 1 } }));

    expect(text(el, 'beer-rating')).toBe('5.0 (1 review)');
  });

  it('shows one price when every tier costs the same', () => {
    const el = render(
      makeBeer({
        pricing: [{ price: 7, quantity: 1, unitVolume: 12, packaging: 'Can' }],
      }),
    );

    expect(text(el, 'beer-price')).toBe('$7.00');
  });

  it.each([
    ['null', null],
    ['unreviewed', { average: 0, reviewCount: 0 }],
  ])('shows "No reviews yet" for a %s rating', (_label, rating) => {
    expect(text(render(makeBeer({ rating })), 'beer-rating')).toBe(
      'No reviews yet',
    );
  });

  it.each([
    ['null', null],
    ['empty', []],
  ])('shows "Price unavailable" for %s pricing', (_label, pricing) => {
    expect(text(render(makeBeer({ pricing })), 'beer-price')).toBe(
      'Price unavailable',
    );
  });

  it('shows a placeholder icon when there is no image', () => {
    const el = render(makeBeer({ image: '' }));

    expect(el.querySelector('img')).toBeNull();
    expect(el.querySelector('figure i')).toBeTruthy();
  });
});
