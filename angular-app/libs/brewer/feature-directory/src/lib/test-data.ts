import type { Brewer } from '@beers/brewer/data-access';

export const guid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c33${String(n).padStart(2, '0')}`;

export const MICRO = { id: guid(90), name: 'Micro' };
export const REGIONAL = { id: guid(91), name: 'Regional' };
export const BREWPUB = { id: guid(92), name: 'Brewpub' };

export function makeBrewer(overrides: Partial<Brewer> = {}): Brewer {
  return {
    brewerId: guid(1),
    name: 'Odell',
    foundedIn: 1989,
    headquarters: 'Fort Collins, CO',
    website: 'https://odell.test',
    isDeletable: false,
    breweryType: MICRO,
    rating: { average: 4.5, reviewCount: 8 },
    createdDate: '2026-01-01T00:00:00Z',
    modifiedDate: '2026-01-02T00:00:00Z',
    ...overrides,
  };
}

/** `count` brewers named "Brewer 01", "Brewer 02", ... where every third one is Regional. */
export function makeBrewers(count: number): Brewer[] {
  return Array.from({ length: count }, (_, i) =>
    makeBrewer({
      brewerId: guid(i + 1),
      name: `Brewer ${String(i + 1).padStart(2, '0')}`,
      breweryType: (i + 1) % 3 === 0 ? REGIONAL : MICRO,
    }),
  );
}
