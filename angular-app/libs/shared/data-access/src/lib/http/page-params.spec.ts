import { toPageParams } from './page-params';

describe('toPageParams', () => {
  it('maps every field to its PascalCase key', () => {
    const params = toPageParams({
      pageNumber: 3,
      pageSize: 20,
      sortOrder: 'desc',
      orderBy: 'name',
    });
    expect(params.keys().sort()).toEqual([
      'OrderBy',
      'PageNumber',
      'PageSize',
      'SortOrder',
    ]);
    expect(params.get('PageNumber')).toBe('3');
    expect(params.get('PageSize')).toBe('20');
    expect(params.get('SortOrder')).toBe('desc');
    expect(params.get('OrderBy')).toBe('name');
  });

  it('omits fields that are not set', () => {
    expect(toPageParams({}).keys()).toEqual([]);
  });

  it.each([null, undefined, 'page', 42])(
    'returns empty params for the non-object %j',
    (page) => {
      expect(
        toPageParams(
          page as unknown as Parameters<typeof toPageParams>[0],
        ).keys(),
      ).toEqual([]);
    },
  );

  it.each([NaN, Infinity, -Infinity])('drops the non-finite number %d', (n) => {
    const params = toPageParams({ pageNumber: n, pageSize: n });
    expect(params.has('PageNumber')).toBe(false);
    expect(params.has('PageSize')).toBe(false);
  });

  it.each([
    [0, '1'],
    [-4, '1'],
    [2.9, '2'],
    [7, '7'],
  ])('floors PageNumber %d at %s', (pageNumber, expected) => {
    expect(toPageParams({ pageNumber }).get('PageNumber')).toBe(expected);
  });

  it.each([
    [0, '1'],
    [-3, '1'],
    [0.5, '1'],
    [20.9, '20'],
    [50, '50'],
    [51, '50'],
    [500, '50'],
  ])('clamps PageSize %d to %s', (pageSize, expected) => {
    expect(toPageParams({ pageSize }).get('PageSize')).toBe(expected);
  });
});
