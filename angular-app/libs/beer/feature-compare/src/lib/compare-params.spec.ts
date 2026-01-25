import { convertToParamMap } from '@angular/router';
import { parseCompareParams } from './compare-params';

const guid = (n: number) =>
  `3f2504e0-4f89-11d3-9a0c-0305e82c33${String(n).padStart(2, '0')}`;
const [A, B, C, D, E] = [1, 2, 3, 4, 5].map(guid);

const parse = (beers?: string) =>
  parseCompareParams(convertToParamMap(beers === undefined ? {} : { beers }));

describe('parseCompareParams', () => {
  describe('ids', () => {
    it.each([
      ['one id', A, [A]],
      ['two ids, in URL order', `${B},${A}`, [B, A]],
      ['uppercase ids, lowercased', `${A.toUpperCase()},${B}`, [A, B]],
      ['segments with spaces, trimmed', `${A}, ${B} `, [A, B]],
      ['a repeated id, once', `${A},${B},${A}`, [A, B]],
      ['the same id in another case, once', `${A},${A.toUpperCase()}`, [A]],
      ['junk dropped, valid kept', `${A},junk,,${B}`, [A, B]],
      ['an empty value', '', []],
      ['only junk', 'junk,123', []],
      ['a GUID with an extra character', `${A}x,${B}`, [B]],
      ['a trailing comma', `${A},${B},`, [A, B]],
    ])('%s', (_, value, ids) => {
      expect(parse(value).ids).toEqual(ids);
    });

    it('is empty when there is no beers param', () => {
      expect(parse()).toEqual({ ids: [], overCap: false, hasParam: false });
    });

    it('reads only the first of a repeated beers param, as the story has one list', () => {
      const params = convertToParamMap({ beers: [A, B] });

      expect(parseCompareParams(params).ids).toEqual([A]);
    });
  });

  describe('hasParam', () => {
    it.each([
      ['absent', undefined, false],
      ['present and empty', '', true],
      ['present and invalid', 'junk', true],
      ['present and valid', A, true],
    ])('is %s', (_, value, hasParam) => {
      expect(parse(value).hasParam).toBe(hasParam);
    });
  });

  describe('the cap', () => {
    it('keeps the first 4 and reports overCap for 5 unique ids', () => {
      const result = parse([A, B, C, D, E].join(','));

      expect(result.ids).toEqual([A, B, C, D]);
      expect(result.overCap).toBe(true);
    });

    it('is not over the cap at exactly 4', () => {
      const result = parse([A, B, C, D].join(','));

      expect(result.ids).toEqual([A, B, C, D]);
      expect(result.overCap).toBe(false);
    });

    // Duplicates are not extra beers, so they cannot push a list over the cap.
    it('counts unique ids only: A,a,b,c,d is 4 unique and not over', () => {
      const result = parse(`${A},${A.toUpperCase()},${B},${C},${D}`);

      expect(result.ids).toEqual([A, B, C, D]);
      expect(result.overCap).toBe(false);
    });

    it('counts unique ids only: A,a,b,c,d,e is 5 unique and over', () => {
      const result = parse(`${A},${A.toUpperCase()},${B},${C},${D},${E}`);

      expect(result.ids).toEqual([A, B, C, D]);
      expect(result.overCap).toBe(true);
    });

    it('does not count junk toward the cap', () => {
      const result = parse(`${A},junk,${B},x,${C},y,${D}`);

      expect(result.ids).toEqual([A, B, C, D]);
      expect(result.overCap).toBe(false);
    });
  });
});
