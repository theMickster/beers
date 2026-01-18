import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
} from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_BASE_URL, type PagedResult } from '@beers/shared/data-access';
import { catchError, of, throwError, type Observable } from 'rxjs';
import type {
  BrewerReview,
  BrewerReviewPageQuery,
  BrewerReviewSearchFilters,
  CreateBrewerReview,
} from './brewer-review.models';
import {
  validateCreateBrewerReview,
  validateReviewId,
  validateReviewSearch,
} from './brewer-review.validators';
import { validateBrewerId } from './brewer.validators';
import { isObject, MAX_PAGE_SIZE, whenValid } from './validation';

const TEXT_FILTER_KEYS = ['id', 'name', 'reviewerName'] as const;
const RATING_FILTER_KEYS = ['minimumRating', 'maximumRating'] as const;

/**
 * Thin HTTP wrapper over the brewer review endpoints. No state, no error mapping.
 *
 * Every method validates first and fails with a client `ApiError` without sending a request
 * when the input breaks a server rule. The API has no review update endpoint.
 */
@Injectable({ providedIn: 'root' })
export class BrewerReviewApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /** Unpaged list. The API answers 404 when there are no reviews, which becomes `[]`. */
  list(brewerId: string): Observable<readonly BrewerReview[]> {
    return whenValid(validateBrewerId(brewerId), () =>
      this.http
        .get<BrewerReview[]>(this.reviewsUrl(brewerId))
        .pipe(
          catchError((error: unknown) =>
            error instanceof HttpErrorResponse && error.status === 404
              ? of([])
              : throwError(() => error),
          ),
        ),
    );
  }

  getById(brewerId: string, reviewId: string): Observable<BrewerReview> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateReviewId(reviewId),
    ];
    return whenValid(violations, () =>
      this.http.get<BrewerReview>(this.reviewUrl(brewerId, reviewId)),
    );
  }

  search(
    brewerId: string,
    page: BrewerReviewPageQuery,
    filters: BrewerReviewSearchFilters,
  ): Observable<PagedResult<BrewerReview>> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateReviewSearch(filters),
    ];
    return whenValid(violations, () =>
      this.http.post<PagedResult<BrewerReview>>(
        `${this.reviewsUrl(brewerId)}/search`,
        toSearchBody(filters),
        { params: toPageParams(page) },
      ),
    );
  }

  create(
    brewerId: string,
    review: CreateBrewerReview,
  ): Observable<BrewerReview> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateCreateBrewerReview(review),
    ];
    return whenValid(violations, () =>
      this.http.post<BrewerReview>(this.reviewsUrl(brewerId), {
        reviewerName: review.reviewerName,
        title: review.title,
        comments: review.comments,
        rating: review.rating,
      }),
    );
  }

  delete(brewerId: string, reviewId: string): Observable<void> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateReviewId(reviewId),
    ];
    return whenValid(violations, () =>
      this.http.delete<void>(this.reviewUrl(brewerId, reviewId)),
    );
  }

  private reviewsUrl(brewerId: string): string {
    return `${this.baseUrl}/brewers/${encodeURIComponent(brewerId)}/reviews`;
  }

  private reviewUrl(brewerId: string, reviewId: string): string {
    return `${this.reviewsUrl(brewerId)}/${encodeURIComponent(reviewId)}`;
  }
}

/** Non-finite numbers and a missing `page` are omitted; the server defaults apply. */
function toPageParams(page: BrewerReviewPageQuery): HttpParams {
  let params = new HttpParams();
  if (!isObject(page)) return params;
  if (page.pageNumber !== undefined && Number.isFinite(page.pageNumber)) {
    params = params.set('PageNumber', Math.max(1, Math.trunc(page.pageNumber)));
  }
  if (page.pageSize !== undefined && Number.isFinite(page.pageSize)) {
    params = params.set('PageSize', clampPageSize(page.pageSize));
  }
  if (page.sortOrder !== undefined) {
    params = params.set('SortOrder', page.sortOrder);
  }
  if (page.orderBy !== undefined) {
    params = params.set('OrderBy', page.orderBy);
  }
  return params;
}

/** The server caps at 50 and has no lower bound (zero or less likely fails), so clamp to 1..50. */
function clampPageSize(pageSize: number): number {
  return Math.min(MAX_PAGE_SIZE, Math.max(1, Math.trunc(pageSize)));
}

/** The API requires a body, so an empty filter set still sends `{}`. */
function toSearchBody(
  filters: BrewerReviewSearchFilters,
): Record<string, string | number> {
  const body: Record<string, string | number> = {};
  for (const key of TEXT_FILTER_KEYS) {
    const value = filters[key];
    if (typeof value === 'string' && value.trim() !== '') {
      body[key] = value;
    }
  }
  for (const key of RATING_FILTER_KEYS) {
    const value = filters[key];
    if (value !== undefined) {
      body[key] = value;
    }
  }
  return body;
}
