import {
  isBlank,
  isGuid,
  isObject,
  REQUEST_BODY_REQUIRED,
} from '@beers/shared/data-access';
import type {
  BrewerReviewSearchFilters,
  CreateBrewerReview,
} from './brewer-review.models';
import {
  isRating,
  MAX_RATING,
  MAX_REVIEW_COMMENTS_LENGTH,
  MAX_REVIEW_TITLE_LENGTH,
  MAX_REVIEWER_NAME_LENGTH,
  MIN_RATING,
} from './validation';

/*
 * Client-side mirror of the server's FluentValidation rules. Each function returns the
 * violated rules as messages; an empty array means the input is valid.
 */

export function validateReviewId(reviewId: string): readonly string[] {
  return isGuid(reviewId) ? [] : ['Review id must be a valid GUID.'];
}

export function validateCreateBrewerReview(
  review: CreateBrewerReview,
): readonly string[] {
  if (!isObject(review)) return [REQUEST_BODY_REQUIRED];
  return [
    ...validateText(
      review.reviewerName,
      'Reviewer name',
      MAX_REVIEWER_NAME_LENGTH,
    ),
    ...validateText(review.title, 'Title', MAX_REVIEW_TITLE_LENGTH),
    ...validateText(review.comments, 'Comments', MAX_REVIEW_COMMENTS_LENGTH),
    ...(isRating(review.rating) ? [] : [ratingMessage('Rating')]),
  ];
}

/**
 * Checks the `id` filter and the rating bounds; other blank text filters are dropped by the API
 * service. The 1 to 5 rating bounds are intentionally stricter than the server.
 */
export function validateReviewSearch(
  filters: BrewerReviewSearchFilters,
): readonly string[] {
  if (!isObject(filters)) return [REQUEST_BODY_REQUIRED];
  const { id, minimumRating, maximumRating } = filters;
  const errors: string[] = [];
  if (!isBlank(id) && !isGuid(id)) {
    errors.push('Id filter must be a valid GUID.');
  }
  if (minimumRating !== undefined && !isRating(minimumRating)) {
    errors.push(ratingMessage('Minimum rating'));
  }
  if (maximumRating !== undefined && !isRating(maximumRating)) {
    errors.push(ratingMessage('Maximum rating'));
  }
  if (
    isRating(minimumRating) &&
    isRating(maximumRating) &&
    minimumRating > maximumRating
  ) {
    errors.push('Minimum rating must not be greater than maximum rating.');
  }
  return errors;
}

function validateText(
  value: string,
  label: string,
  maxLength: number,
): readonly string[] {
  if (isBlank(value)) return [`${label} is required.`];
  return value.length > maxLength
    ? [`${label} must be ${maxLength} characters or fewer.`]
    : [];
}

function ratingMessage(label: string): string {
  return `${label} must be a whole number from ${MIN_RATING} to ${MAX_RATING}.`;
}
