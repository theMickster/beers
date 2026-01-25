import type { Beer } from '@beers/beer/data-access';
import type { Brewer, BrewerReview } from '@beers/brewer/data-access';

export const guid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c33${String(n).padStart(2, '0')}`;
export const [A, B] = [1, 2].map(guid);

export function makeBrewer(overrides: Partial<Brewer> = {}): Brewer {
  return {
    brewerId: A,
    name: 'Odell',
    foundedIn: 1989,
    headquarters: 'Fort Collins, CO',
    website: 'https://odell.test',
    isDeletable: false,
    breweryType: { id: guid(90), name: 'Regional' },
    rating: { average: 4.25, reviewCount: 12 },
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
    ...overrides,
  };
}

export function makeBeer(overrides: Partial<Beer> = {}): Beer {
  return {
    beerId: guid(40),
    brewerId: A,
    name: 'Hazy Day',
    description: '',
    image: '',
    sku: 'sku-1',
    isDeletable: false,
    rating: { average: 4, reviewCount: 3 },
    pricing: null,
    brewer: { id: A, name: 'Odell', website: 'https://odell.test' },
    beerType: { id: 't1', name: 'Ale' },
    beerCategories: [],
    beerStyles: [],
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
    ...overrides,
  };
}

export function makeReview(
  overrides: Partial<BrewerReview> = {},
): BrewerReview {
  return {
    reviewId: guid(60),
    brewerId: A,
    reviewerName: 'Sam',
    title: 'Great',
    comments: 'Loved the lager.',
    rating: 4,
    isDeletable: false,
    createdDate: '2026-03-05T10:00:00Z',
    modifiedDate: '2026-03-05T10:00:00Z',
    ...overrides,
  };
}
