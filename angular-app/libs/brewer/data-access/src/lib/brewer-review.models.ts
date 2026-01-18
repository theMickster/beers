/** Mirrors the API's brewer review response (camelCase JSON; dates are ISO strings). */
export interface BrewerReview {
  readonly reviewId: string;
  readonly brewerId: string;
  readonly reviewerName: string;
  readonly title: string;
  readonly comments: string;
  readonly rating: number;
  readonly isDeletable: boolean;
  readonly createdDate: string;
  readonly modifiedDate: string;
}

/** The brewer comes from the route. The API has no review update endpoint, so there is no update type. */
export interface CreateBrewerReview {
  readonly reviewerName: string;
  readonly title: string;
  readonly comments: string;
  readonly rating: number;
}

export type BrewerReviewOrderBy =
  'reviewId' | 'brewerId' | 'reviewerName' | 'rating';

/** The server clamps `pageSize` to 50 (default 10). */
export interface BrewerReviewPageQuery {
  readonly pageNumber?: number;
  readonly pageSize?: number;
  readonly sortOrder?: 'asc' | 'desc';
  readonly orderBy?: BrewerReviewOrderBy;
}

/** The brewer is scoped by the route, so `brewerId` is deliberately not a filter. */
export interface BrewerReviewSearchFilters {
  readonly id?: string;
  readonly name?: string;
  readonly reviewerName?: string;
  readonly minimumRating?: number;
  readonly maximumRating?: number;
}
