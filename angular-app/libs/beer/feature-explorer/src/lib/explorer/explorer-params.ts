import type { ParamMap } from '@angular/router';
import { isGuid, normalizeId } from '@beers/shared/data-access';

/** The server clamps `pageSize` at 50; these are the sizes the page offers. */
export const PAGE_SIZE_OPTIONS = [12, 24, 48] as const;
export const DEFAULT_PAGE_SIZE: number = PAGE_SIZE_OPTIONS[0];

/** The API answers 400 for an id list longer than this. */
const MAX_IDS_PER_FILTER = 25;

/** Keeps `(page - 1) * pageSize` and the page number inside the API's int range. */
const MAX_PAGE = 1_000_000;

const NON_NEGATIVE_NUMBER = /^\d+(\.\d+)?$/;
const POSITIVE_INTEGER = /^\d+$/;

export type FacetKey = 'style' | 'category' | 'type';

export interface ExplorerParams {
  readonly styleIds: readonly string[];
  readonly categoryIds: readonly string[];
  readonly typeIds: readonly string[];
  readonly minPrice: number | undefined;
  readonly maxPrice: number | undefined;
  readonly page: number;
  readonly pageSize: number;
}

/**
 * Reads the explorer state from the URL. The URL is user-editable and the API answers 400 for
 * malformed GUIDs, oversized lists and inverted price ranges, so anything unusable is dropped.
 */
export function parseExplorerParams(params: ParamMap): ExplorerParams {
  const minPrice = parsePrice(params.get('minPrice'));
  const maxPrice = parsePrice(params.get('maxPrice'));
  const invertedRange =
    minPrice !== undefined && maxPrice !== undefined && minPrice > maxPrice;

  return {
    styleIds: parseIds(params.getAll('style')),
    categoryIds: parseIds(params.getAll('category')),
    typeIds: parseIds(params.getAll('type')),
    minPrice: invertedRange ? undefined : minPrice,
    maxPrice: invertedRange ? undefined : maxPrice,
    page: parsePage(params.get('page')),
    pageSize: parsePageSize(params.get('pageSize')),
  };
}

/** Parses a price typed by the user. Blank is "no bound"; anything else must be a valid price. */
export function parsePrice(value: string | null): number | undefined {
  const text = value?.trim() ?? '';
  if (!NON_NEGATIVE_NUMBER.test(text)) return undefined;
  const price = Number(text);
  return Number.isFinite(price) ? price : undefined;
}

function parseIds(values: readonly string[]): string[] {
  const ids = values.filter(isGuid).map(normalizeId);
  return [...new Set(ids)].slice(0, MAX_IDS_PER_FILTER);
}

function parsePage(value: string | null): number {
  const text = value?.trim() ?? '';
  if (!POSITIVE_INTEGER.test(text)) return 1;
  return Math.min(Math.max(Number(text), 1), MAX_PAGE);
}

function parsePageSize(value: string | null): number {
  const size = Number(value);
  return (
    PAGE_SIZE_OPTIONS.find((option) => option === size) ?? DEFAULT_PAGE_SIZE
  );
}

/** Page numbers to show, with `null` standing in for a gap: 1 … 4 5 6 … 12. */
export function pageWindow(current: number, total: number): (number | null)[] {
  const pages = new Set(
    [1, current - 1, current, current + 1, total].filter(
      (page) => page >= 1 && page <= total,
    ),
  );
  const sorted = [...pages].sort((a, b) => a - b);
  const result: (number | null)[] = [];
  for (const page of sorted) {
    const previous = result[result.length - 1];
    if (typeof previous === 'number' && page - previous > 1) {
      result.push(null);
    }
    result.push(page);
  }
  return result;
}
