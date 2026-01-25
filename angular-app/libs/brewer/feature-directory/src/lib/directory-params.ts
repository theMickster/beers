import type { ParamMap } from '@angular/router';
import type { Brewer } from '@beers/brewer/data-access';
import { normalizeId } from '@beers/shared/data-access';

/** The story gives no page size, so 6 is an assumption. The API has no brewer paging. */
export const DIRECTORY_PAGE_SIZE = 6;

const POSITIVE_INTEGER = /^\d+$/;

export interface DirectoryParams {
  /** The raw `type` from the URL, which may name a brewery type that no brewer has. */
  readonly type: string | null;
  readonly page: number;
}

type BreweryType = Brewer['breweryType'];

export interface DirectoryPage {
  readonly items: readonly Brewer[];
  /** The page actually shown, after clamping to the pages that exist. */
  readonly page: number;
  readonly totalPages: number;
  /** Only the types some brewer has, so the filter never offers an option that matches nothing. */
  readonly types: readonly BreweryType[];
  /** The id of the type filtering the list, or `null` for "All types". */
  readonly type: string | null;
}

/** Reads the directory state from the URL. It is user-editable, so anything unusable is dropped. */
export function parseDirectoryParams(params: ParamMap): DirectoryParams {
  const page = POSITIVE_INTEGER.test(params.get('page') ?? '')
    ? Number(params.get('page'))
    : 0;
  return { type: params.get('type') || null, page: Math.max(page, 1) };
}

/**
 * Filters by brewery type, sorts by name and slices one page. The API has no brewer search or
 * paging, so this runs on the whole list. A `type` that no brewer has reads as "All types"
 * instead of an empty list, and a `page` past the end shows the last page.
 */
export function selectDirectoryPage(
  brewers: readonly Brewer[],
  params: DirectoryParams,
  pageSize = DIRECTORY_PAGE_SIZE,
): DirectoryPage {
  const types = [
    ...new Map(
      brewers.map((b) => [normalizeId(b.breweryType.id), b.breweryType]),
    ).values(),
  ].sort(byName);

  const type =
    types.find((t) => normalizeId(t.id) === normalizeId(params.type ?? ''))
      ?.id ?? null;
  const matching = brewers
    .filter(
      (b) =>
        type === null || normalizeId(b.breweryType.id) === normalizeId(type),
    )
    .sort(byName);

  const totalPages = Math.ceil(matching.length / pageSize);
  const page = Math.max(Math.min(params.page, totalPages), 1);
  return {
    items: matching.slice((page - 1) * pageSize, page * pageSize),
    page,
    totalPages,
    types,
    type,
  };
}

function byName(a: { name: string }, b: { name: string }): number {
  return a.name.localeCompare(b.name);
}
