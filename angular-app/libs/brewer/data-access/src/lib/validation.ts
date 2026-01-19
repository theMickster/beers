export const MAX_NAME_LENGTH = 256;
export const MAX_HEADQUARTERS_LENGTH = 256;
export const MAX_WEBSITE_LENGTH = 500;
export const MIN_FOUNDED_IN = 1500;

export const MAX_REVIEWER_NAME_LENGTH = 128;
export const MAX_REVIEW_TITLE_LENGTH = 256;
export const MAX_REVIEW_COMMENTS_LENGTH = 4000;
export const MIN_RATING = 1;
export const MAX_RATING = 5;

export function isRating(value: unknown): value is number {
  return (
    typeof value === 'number' &&
    Number.isInteger(value) &&
    value >= MIN_RATING &&
    value <= MAX_RATING
  );
}
