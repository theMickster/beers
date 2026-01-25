import type { ParamMap } from '@angular/router';
import { MAX_COMPARE_BEERS } from '@beers/beer/data-access';
import { isGuid, normalizeId } from '@beers/shared/data-access';

export interface CompareParams {
  /** Unique, lowercase GUIDs in URL order, at most the cap. */
  readonly ids: string[];
  /** True when more unique valid ids were present than the cap allows. */
  readonly overCap: boolean;
  /** True when `beers` is in the URL at all, valid or not. A bare `/compare` has none. */
  readonly hasParam: boolean;
}

/**
 * Reads `?beers=id1,id2` from the URL. The URL is user-editable and the API answers 404 for ids
 * that are not GUIDs, so anything unusable is dropped before a request is made.
 */
export function parseCompareParams(params: ParamMap): CompareParams {
  const unique = new Set(
    (params.get('beers') ?? '')
      .split(',')
      .map((segment) => segment.trim())
      .filter(isGuid)
      .map(normalizeId),
  );
  return {
    ids: [...unique].slice(0, MAX_COMPARE_BEERS),
    overCap: unique.size > MAX_COMPARE_BEERS,
    hasParam: params.has('beers'),
  };
}
