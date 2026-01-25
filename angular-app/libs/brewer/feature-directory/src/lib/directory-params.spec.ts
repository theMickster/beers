import { convertToParamMap } from '@angular/router';
import type { Brewer } from '@beers/brewer/data-access';
import {
  parseDirectoryParams,
  selectDirectoryPage,
  type DirectoryParams,
} from './directory-params';
import {
  BREWPUB,
  guid,
  makeBrewer,
  makeBrewers,
  MICRO,
  REGIONAL,
} from './test-data';

const parse = (query: Record<string, string>) =>
  parseDirectoryParams(convertToParamMap(query));

describe('parseDirectoryParams', () => {
  it.each([
    ['nothing', {}, { type: null, page: 1 }],
    [
      'a type and a page',
      { type: MICRO.id, page: '3' },
      { type: MICRO.id, page: 3 },
    ],
    ['an empty type', { type: '' }, { type: null, page: 1 }],
    [
      'a type that is not a GUID, kept raw for the caller to judge',
      { type: 'junk' },
      { type: 'junk', page: 1 },
    ],
    ['page 0', { page: '0' }, { type: null, page: 1 }],
    ['a negative page', { page: '-2' }, { type: null, page: 1 }],
    ['a fractional page', { page: '2.5' }, { type: null, page: 1 }],
    ['a page that is not a number', { page: 'abc' }, { type: null, page: 1 }],
    ['an empty page', { page: '' }, { type: null, page: 1 }],
    [
      'a page of digits too long to be a safe integer',
      { page: '9'.repeat(30) },
      { type: null, page: 1e30 },
    ],
  ])('reads %s', (_, query, expected) => {
    expect(parse(query)).toEqual(expected);
  });
});

describe('selectDirectoryPage', () => {
  const select = (
    brewers: readonly Brewer[],
    params: Partial<DirectoryParams> = {},
    pageSize?: number,
  ) =>
    selectDirectoryPage(brewers, { type: null, page: 1, ...params }, pageSize);

  describe('paging', () => {
    it('slices 6 brewers a page by default', () => {
      const result = select(makeBrewers(10));

      expect(result.items.map((b) => b.name)).toEqual([
        'Brewer 01',
        'Brewer 02',
        'Brewer 03',
        'Brewer 04',
        'Brewer 05',
        'Brewer 06',
      ]);
      expect(result.totalPages).toBe(2);
      expect(result.page).toBe(1);
    });

    it('shows the remainder on the last page', () => {
      const result = select(makeBrewers(10), { page: 2 });

      expect(result.items.map((b) => b.name)).toEqual([
        'Brewer 07',
        'Brewer 08',
        'Brewer 09',
        'Brewer 10',
      ]);
    });

    it('honours another page size', () => {
      const result = select(makeBrewers(10), { page: 2 }, 4);

      expect(result.items).toHaveLength(4);
      expect(result.totalPages).toBe(3);
    });

    it('fills exactly one page with no extra', () => {
      const result = select(makeBrewers(6));

      expect(result.items).toHaveLength(6);
      expect(result.totalPages).toBe(1);
    });

    it.each([
      ['past the end', 99, 2],
      ['below the first', 0, 1],
    ])('clamps a page %s to %i', (_, page, expected) => {
      expect(select(makeBrewers(10), { page }).page).toBe(expected);
    });

    it('has no pages and page 1 when there are no brewers', () => {
      expect(select([], { page: 4 })).toEqual({
        items: [],
        page: 1,
        totalPages: 0,
        types: [],
        type: null,
      });
    });
  });

  describe('sorting', () => {
    it('sorts by name regardless of the order the API sent, ignoring case', () => {
      const brewers = ['charlie', 'Alpha', 'bravo'].map((name, i) =>
        makeBrewer({ brewerId: guid(i + 1), name }),
      );

      expect(select(brewers).items.map((b) => b.name)).toEqual([
        'Alpha',
        'bravo',
        'charlie',
      ]);
    });

    it('does not reorder the list it was given', () => {
      const brewers = ['b', 'a'].map((name, i) =>
        makeBrewer({ brewerId: guid(i + 1), name }),
      );

      select(brewers);

      expect(brewers.map((b) => b.name)).toEqual(['b', 'a']);
    });
  });

  describe('the type filter', () => {
    it('offers only the types some brewer has, once each', () => {
      const result = select(makeBrewers(10));

      expect(result.types).toEqual([MICRO, REGIONAL]);
    });

    // The first brewer by name has the type that sorts last, so list order and name order differ.
    it('sorts the types by name, not by the order the brewers list them', () => {
      const brewers = [
        makeBrewer({ brewerId: guid(1), name: 'A', breweryType: REGIONAL }),
        makeBrewer({ brewerId: guid(2), name: 'B', breweryType: MICRO }),
      ];

      expect(select(brewers).types).toEqual([MICRO, REGIONAL]);
    });

    it('keeps only brewers of the chosen type, then pages them', () => {
      const result = select(makeBrewers(10), { type: REGIONAL.id });

      expect(result.items.map((b) => b.name)).toEqual([
        'Brewer 03',
        'Brewer 06',
        'Brewer 09',
      ]);
      expect(result.type).toBe(REGIONAL.id);
      expect(result.totalPages).toBe(1);
    });

    it('pages within the filtered list, not the whole one', () => {
      const result = select(makeBrewers(30), { type: MICRO.id, page: 2 }, 6);

      expect(result.totalPages).toBe(4);
      expect(result.items[0].name).toBe('Brewer 10');
    });

    it('matches the type id in any case', () => {
      const result = select(makeBrewers(10), {
        type: REGIONAL.id.toUpperCase(),
      });

      expect(result.items).toHaveLength(3);
      expect(result.type).toBe(REGIONAL.id);
    });

    // A shared link can name a type that no brewer has; an empty list would look like a bug.
    it('reads a type that no brewer has as "All types"', () => {
      const result = select(makeBrewers(10), { type: BREWPUB.id });

      expect(result.type).toBeNull();
      expect(result.items).toHaveLength(6);
      expect(result.totalPages).toBe(2);
    });

    it('reads a type that is not a GUID as "All types"', () => {
      expect(select(makeBrewers(10), { type: 'junk' }).type).toBeNull();
    });

    it('clamps the page against the filtered list', () => {
      const result = select(makeBrewers(10), { type: REGIONAL.id, page: 5 });

      expect(result.page).toBe(1);
      expect(result.items).toHaveLength(3);
    });
  });
});
