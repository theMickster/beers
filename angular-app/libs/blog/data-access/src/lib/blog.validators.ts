import {
  isBlank,
  isGuid,
  isObject,
  REQUEST_BODY_REQUIRED,
} from '@beers/shared/data-access';
import {
  POST_TYPES,
  type BlogPostSearchFilters,
  type CreateNewsBlogPost,
  type PostType,
} from './blog.models';

/*
 * Client-side mirror of the server's FluentValidation rules. Each function returns the
 * violated rules as messages; an empty array means the input is valid.
 */

const MAX_TITLE_LENGTH = 256;
const MAX_BODY_LENGTH = 4000;

/** The ISO 8601 forms System.Text.Json reads into a `DateTime`; `Date.parse` accepts more. */
const ISO_DATE_PATTERN =
  /^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}):(\d{2})(?::(\d{2})(?:\.\d{1,16})?)?(?:Z|[+-](\d{2}):(\d{2}))?)?$/;

const DAYS_IN_MONTH = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

/** The server rejects a UTC offset beyond +/-14:00. */
const MAX_OFFSET_MINUTES = 14 * 60;

export function validatePostId(postId: string): readonly string[] {
  return isGuid(postId) ? [] : ['Post id must be a valid GUID.'];
}

export function validateCreatePost(
  post: CreateNewsBlogPost,
): readonly string[] {
  if (!isObject(post)) return [REQUEST_BODY_REQUIRED];
  const postType = parsePostType(post.postType);
  return [
    ...validateTitle(post.title),
    ...validatePostType(post.postType),
    ...validateOptionalDate(post.eventDate, 'Event date'),
    ...validateOptionalDate(post.publishedDate, 'Published date'),
    ...(postType === 'EventAnnouncement' ? validateEvent(post) : []),
    ...(postType === 'TextPost' ? validateBody(post.body) : []),
    ...(postType === 'ImageGallery' ? validateImageUrls(post.imageUrls) : []),
  ];
}

/** The ids come from the route, so a PUT body can never disagree with them. */
export function validateUpdatePost(
  postId: string,
  post: CreateNewsBlogPost,
): readonly string[] {
  return [...validatePostId(postId), ...validateCreatePost(post)];
}

/**
 * The API silently ignores an unknown `postType`, so it is rejected here. Blank text filters and
 * non-string values are dropped by the API service, as in the review search. The end-before-start
 * check is stricter than the server, which never runs its own range validator on this endpoint.
 */
export function validatePostSearch(
  filters: BlogPostSearchFilters,
): readonly string[] {
  if (!isObject(filters)) return [REQUEST_BODY_REQUIRED];
  const { brewerId, postType, dateRangeStart, dateRangeEnd } = filters;
  const errors: string[] = [];
  if (!isBlank(brewerId) && !isGuid(brewerId)) {
    errors.push('Brewer id filter must be a valid GUID.');
  }
  if (!isBlank(postType) && parsePostType(postType) === null) {
    errors.push(postTypeMessage());
  }
  if (!isBlank(dateRangeStart) && !isIsoDate(dateRangeStart)) {
    errors.push(isoDateMessage('Date range start'));
  }
  if (!isBlank(dateRangeEnd) && !isIsoDate(dateRangeEnd)) {
    errors.push(isoDateMessage('Date range end'));
  }
  if (
    isIsoDate(dateRangeStart) &&
    isIsoDate(dateRangeEnd) &&
    toUtcTimestamp(dateRangeEnd) < toUtcTimestamp(dateRangeStart)
  ) {
    errors.push(
      'DateRangeEnd must be greater than or equal to DateRangeStart.',
    );
  }
  return errors;
}

function validateTitle(title: string): readonly string[] {
  if (isBlank(title)) return ['Title cannot be null, empty, or whitespace'];
  return title.length > MAX_TITLE_LENGTH
    ? [`Title cannot be longer than ${MAX_TITLE_LENGTH} characters`]
    : [];
}

function validatePostType(postType: string): readonly string[] {
  if (isBlank(postType)) {
    return ['Post type cannot be null, empty, or whitespace'];
  }
  return parsePostType(postType) === null ? [postTypeMessage()] : [];
}

function validateEvent(post: CreateNewsBlogPost): readonly string[] {
  return [
    ...(post.eventDate == null
      ? ['Event date is required for EventAnnouncement posts']
      : []),
    ...(isBlank(post.eventLocation)
      ? ['Event location is required for EventAnnouncement posts']
      : []),
  ];
}

function validateBody(body: string): readonly string[] {
  if (isBlank(body)) return ['Body cannot be null, empty, or whitespace'];
  return body.length > MAX_BODY_LENGTH
    ? [`Body cannot be longer than ${MAX_BODY_LENGTH} characters`]
    : [];
}

function validateImageUrls(imageUrls: readonly string[]): readonly string[] {
  return Array.isArray(imageUrls) && imageUrls.length > 0
    ? []
    : ['ImageUrls must contain at least one url for ImageGallery posts'];
}

/** A missing value is the required-field rule's concern; a present one must be a real date. */
function validateOptionalDate(
  value: string | null | undefined,
  label: string,
): readonly string[] {
  return value == null || isIsoDate(value) ? [] : [isoDateMessage(label)];
}

/** The server matches the name ignoring case. */
function parsePostType(value: unknown): PostType | null {
  if (typeof value !== 'string') return null;
  const lower = value.toLowerCase();
  return POST_TYPES.find((type) => type.toLowerCase() === lower) ?? null;
}

function postTypeMessage(): string {
  return `Post type must be a valid value (${POST_TYPES.join(', ')})`;
}

function isoDateMessage(label: string): string {
  return `${label} must be an ISO 8601 date, such as 2026-05-01 or 2026-05-01T18:30:00Z.`;
}

/**
 * `Date.parse` reads a date-time with no offset as local time. The server leaves it unspecified,
 * so compare it as UTC to match a date-only value.
 */
function toUtcTimestamp(value: string): number {
  const hasTimeWithoutOffset =
    value.includes('T') && !/(?:Z|[+-]\d{2}:\d{2})$/.test(value);
  return Date.parse(hasTimeWithoutOffset ? `${value}Z` : value);
}

/** Checks the pattern and the calendar, clock and UTC offset ranges the server enforces. */
function isIsoDate(value: unknown): value is string {
  if (typeof value !== 'string') return false;
  const match = ISO_DATE_PATTERN.exec(value);
  if (match === null) return false;
  const [year, month, day, hour, minute, second, offsetHour, offsetMinute] =
    match.slice(1).map((part) => (part === undefined ? 0 : Number(part)));
  return (
    year >= 1 &&
    month >= 1 &&
    month <= 12 &&
    day >= 1 &&
    day <= daysInMonth(year, month) &&
    hour <= 23 &&
    minute <= 59 &&
    second <= 59 &&
    offsetMinute <= 59 &&
    offsetHour * 60 + offsetMinute <= MAX_OFFSET_MINUTES
  );
}

function daysInMonth(year: number, month: number): number {
  const isLeapYear = year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
  return month === 2 && isLeapYear ? 29 : (DAYS_IN_MONTH[month - 1] ?? 0);
}
