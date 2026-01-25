import { ApiError } from '@beers/shared/data-access';
import type { Beer, CompareColumn } from '@beers/beer/data-access';

export const guid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c33${String(n).padStart(2, '0')}`;
export const [A, B, C, D, E, F] = [1, 2, 3, 4, 5, 6].map(guid);

export function makeBeer(overrides: Partial<Beer> = {}): Beer {
  return {
    beerId: A,
    brewerId: B,
    name: 'Hazy Day',
    description: '',
    image: 'https://img.test/hazy.png',
    sku: 'sku-1',
    isDeletable: true,
    rating: { average: 4.25, reviewCount: 12 },
    pricing: [{ price: 9.99, quantity: 6, unitVolume: 12, packaging: 'Can' }],
    brewer: { id: B, name: 'Odell', website: 'https://odell.test' },
    beerType: { id: 't1', name: 'Ale' },
    beerCategories: [],
    beerStyles: [{ id: 's1', name: 'IPA' }],
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
    ...overrides,
  };
}

export function loaded(overrides: Partial<Beer> = {}): CompareColumn {
  const beer = makeBeer(overrides);
  return { beerId: beer.beerId, beer, error: null };
}

export function failed(beerId: string, status: number): CompareColumn {
  return {
    beerId,
    beer: null,
    error: new ApiError(status, ['Mock failure.'], 'client'),
  };
}
