import { convertToParamMap, type Params } from '@angular/router';
import { GUID_A, GUID_B } from '../test-data';
import {
  DEFAULT_PAGE_SIZE,
  pageWindow,
  parseExplorerParams,
  parsePrice,
} from './explorer-params';

const parse = (params: Params) =>
  parseExplorerParams(convertToParamMap(params));

describe('parseExplorerParams', () => {
  it('uses defaults for an empty query string', () => {
    expect(parse({})).toEqual({
      styleIds: [],
      categoryIds: [],
      typeIds: [],
      minPrice: undefined,
      maxPrice: undefined,
      page: 1,
      pageSize: DEFAULT_PAGE_SIZE,
    });
  });

  it('reads repeated and single id params, lowercased and de-duplicated', () => {
    const result = parse({
      style: [GUID_A, GUID_B.toUpperCase(), GUID_A],
      type: GUID_A,
    });

    expect(result.styleIds).toEqual([GUID_A, GUID_B]);
    expect(result.typeIds).toEqual([GUID_A]);
  });

  it('drops values that are not GUIDs', () => {
    const result = parse({ style: ['nope', GUID_A, ''], category: 'x' });

    expect(result.styleIds).toEqual([GUID_A]);
    expect(result.categoryIds).toEqual([]);
  });

  it('keeps at most 25 ids per list', () => {
    const ids = Array.from(
      { length: 30 },
      (_, i) => `3f2504e0-4f89-11d3-9a0c-${String(i).padStart(12, '0')}`,
    );

    expect(parse({ style: ids }).styleIds).toHaveLength(25);
  });

  it.each([
    ['0', 1],
    ['-3', 1],
    ['abc', 1],
    ['2.5', 1],
    ['', 1],
    ['7', 7],
    ['99999999999999', 1_000_000],
  ])('reads page %j as %i', (page, expected) => {
    expect(parse({ page }).page).toBe(expected);
  });

  it.each([
    ['12', 12],
    ['24', 24],
    ['48', 48],
    ['50', DEFAULT_PAGE_SIZE],
    ['100', DEFAULT_PAGE_SIZE],
    ['abc', DEFAULT_PAGE_SIZE],
  ])('reads pageSize %j as %i', (pageSize, expected) => {
    expect(parse({ pageSize }).pageSize).toBe(expected);
  });

  it('reads valid prices', () => {
    expect(parse({ minPrice: '5', maxPrice: '12.5' })).toMatchObject({
      minPrice: 5,
      maxPrice: 12.5,
    });
  });

  it('drops an inverted price range', () => {
    expect(parse({ minPrice: '20', maxPrice: '5' })).toMatchObject({
      minPrice: undefined,
      maxPrice: undefined,
    });
  });
});

describe('parsePrice', () => {
  it.each(['0', '5', '12.50', ' 3 '])('accepts %j', (value) => {
    expect(parsePrice(value)).toBe(Number(value.trim()));
  });

  it.each([
    null,
    '',
    '  ',
    '-1',
    'abc',
    'Infinity',
    '1e3',
    '0x10',
    'NaN',
    '1,5',
  ])('rejects %j', (value) => {
    expect(parsePrice(value)).toBeUndefined();
  });
});

describe('pageWindow', () => {
  it('lists every page when there are few', () => {
    expect(pageWindow(1, 3)).toEqual([1, 2, 3]);
  });

  it('shows gaps around the current page', () => {
    expect(pageWindow(6, 12)).toEqual([1, null, 5, 6, 7, null, 12]);
  });

  it('does not add a gap between neighbouring pages', () => {
    expect(pageWindow(2, 12)).toEqual([1, 2, 3, null, 12]);
  });

  it('is empty when there are no pages', () => {
    expect(pageWindow(1, 0)).toEqual([]);
  });
});
