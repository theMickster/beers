import type { Beer } from '@beers/beer/data-access';

export const GUID_A = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
export const GUID_B = '3f2504e0-4f89-11d3-9a0c-0305e82c3302';

export function makeBeer(overrides: Partial<Beer> = {}): Beer {
  return {
    beerId: 'beer-1',
    brewerId: 'br-1',
    name: 'Hazy Day',
    description: 'desc',
    image: 'https://img.test/hazy.png',
    sku: 'sku-1',
    isDeletable: true,
    rating: { average: 4.25, reviewCount: 12 },
    pricing: [
      { price: 9.99, quantity: 6, unitVolume: 12, packaging: 'Can' },
      { price: 18, quantity: 12, unitVolume: 12, packaging: 'Can' },
    ],
    brewer: { id: 'br-1', name: 'Odell', website: 'https://odell.test' },
    beerType: { id: 't1', name: 'Ale' },
    beerCategories: [],
    beerStyles: [],
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
    ...overrides,
  };
}
