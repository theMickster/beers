import { ApiError } from '@beers/shared/data-access';
import { throwError, type Observable } from 'rxjs';

export const MAX_NAME_LENGTH = 256;
export const MAX_HEADQUARTERS_LENGTH = 256;
export const MAX_WEBSITE_LENGTH = 500;
export const MIN_FOUNDED_IN = 1500;

export const MAX_REVIEWER_NAME_LENGTH = 128;
export const MAX_REVIEW_TITLE_LENGTH = 256;
export const MAX_REVIEW_COMMENTS_LENGTH = 4000;
export const MIN_RATING = 1;
export const MAX_RATING = 5;

/** The server caps `PageSize` here and has no lower bound. */
export const MAX_PAGE_SIZE = 50;

export const REQUEST_BODY_REQUIRED = 'Request body is required.';

const GUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

export function isGuid(value: unknown): value is string {
  return typeof value === 'string' && GUID_PATTERN.test(value);
}

/** The server rejects `Guid.Empty` where a GUID is required. */
export function isEmptyGuid(value: string): boolean {
  return value === EMPTY_GUID;
}

/** True for any non-null object, so a missing or malformed request body fails validation instead of throwing. */
export function isObject(value: unknown): value is object {
  return typeof value === 'object' && value !== null;
}

/**
 * The key stores use to compare and index ids. GUIDs are case-insensitive and the server returns
 * them lowercase. A non-string is stringified so it never matches a real id; the API rejects it.
 */
export function normalizeId(id: string): string {
  return typeof id === 'string' ? id.toLowerCase() : String(id);
}

/** True for anything that is not a string with visible characters, so malformed input fails validation instead of throwing. */
export function isBlank(value: unknown): boolean {
  return typeof value !== 'string' || value.trim() === '';
}

export function isRating(value: unknown): value is number {
  return (
    typeof value === 'number' &&
    Number.isInteger(value) &&
    value >= MIN_RATING &&
    value <= MAX_RATING
  );
}

/** Runs `request` only when there are no violations; otherwise fails with a client error and sends nothing. */
export function whenValid<T>(
  violations: readonly string[],
  request: () => Observable<T>,
): Observable<T> {
  return violations.length > 0
    ? throwError(() => new ApiError(400, violations, 'client'))
    : request();
}
