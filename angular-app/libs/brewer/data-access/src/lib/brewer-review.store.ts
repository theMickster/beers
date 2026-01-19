import { inject } from '@angular/core';
import {
  patchState,
  signalStore,
  type,
  withMethods,
  withState,
} from '@ngrx/signals';
import {
  addEntity,
  entityConfig,
  removeAllEntities,
  removeEntity,
  setAllEntities,
  setEntities,
  setEntity,
  withEntities,
} from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  ApiError,
  normalizeId,
  withPagination,
  withRequestStatus,
  type PaginationState,
} from '@beers/shared/data-access';
import {
  catchError,
  defer,
  EMPTY,
  firstValueFrom,
  switchMap,
  tap,
  type Observable,
} from 'rxjs';
import { BrewerReviewApiService } from './brewer-review-api.service';
import type {
  BrewerReview,
  BrewerReviewPageQuery,
  BrewerReviewSearchFilters,
  CreateBrewerReview,
} from './brewer-review.models';

const reviewConfig = entityConfig({
  entity: type<BrewerReview>(),
  selectId: (review: BrewerReview) => normalizeId(review.reviewId),
});

const EMPTY_PAGE: PaginationState<BrewerReview> = {
  results: [],
  pageNumber: 1,
  pageSize: 0,
  totalPages: 0,
  totalRecords: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

interface SearchRequest {
  readonly brewerId: string;
  readonly page: BrewerReviewPageQuery;
  readonly filters: BrewerReviewSearchFilters;
}

interface ReviewRequest {
  readonly brewerId: string;
  readonly reviewId: string;
}

/** Identifies the brewer and the clear-epoch a request started under. */
interface RequestScope {
  readonly brewerKey: string;
  readonly epoch: number;
}

/**
 * Reviews of one active brewer at a time. Every method takes the `brewerId` it acts on; using a
 * different brewer than the last call clears entities, the current page and the status first.
 * Ids compare case-insensitively and are stored lowercase. Responses that arrive after the store
 * was cleared (a brewer switch or `reset()`) are ignored, even when the same brewer is active
 * again, so one brewer's reviews never appear under another, an older request cannot overwrite a
 * newer one's state, and a stale response cannot change `requestStatus`.
 *
 * `list` replaces entities only and leaves the paged `results` alone. `search` fills `results`,
 * paging metadata and entities. create/delete keep entities (and `results` for delete) in sync
 * but do NOT adjust `totalRecords` or `totalPages`, and create never adds to `results`; callers
 * should refetch the page when those matter. There is no update: the API has no review update
 * endpoint. `requestStatus` is one shared status for reads and writes, and request methods never
 * throw: failures land in `error()`. A brewer's aggregate rating is updated by a change feed and
 * can lag behind a newly created review.
 *
 * A write whose brewer is no longer active when it completes is applied to nothing and resolves
 * silently, although the server side effect still happened.
 */
export const BrewerReviewStore = signalStore(
  { providedIn: 'root' },
  withState<{ activeBrewerId: string | null }>({ activeBrewerId: null }),
  withEntities({ entity: type<BrewerReview>() }),
  withPagination<BrewerReview>(),
  withRequestStatus(),
  withMethods((store) => {
    const api = inject(BrewerReviewApiService);
    let epoch = 0;

    function clear(activeBrewerId: string | null): void {
      epoch++;
      patchState(
        store,
        { activeBrewerId, requestStatus: 'idle' as const, ...EMPTY_PAGE },
        removeAllEntities(),
      );
    }

    /** Marks the request as pending for `brewerId`, switching brewers first if needed. */
    function begin(brewerId: string): RequestScope {
      const brewerKey = normalizeId(brewerId);
      if (store.activeBrewerId() !== brewerKey) {
        clear(brewerKey);
      }
      store.setPending();
      return { brewerKey, epoch };
    }

    function whenCurrent(scope: RequestScope, action: () => void): void {
      if (store.activeBrewerId() === scope.brewerKey && epoch === scope.epoch) {
        action();
      }
    }

    function failRead(scope: RequestScope, error: unknown): typeof EMPTY {
      whenCurrent(scope, () => store.setError(ApiError.from(error)));
      return EMPTY;
    }

    const fetchList = rxMethod<string>(
      switchMap((brewerId) => {
        const scope = begin(brewerId);
        return defer(() => api.list(brewerId)).pipe(
          tap((reviews) =>
            whenCurrent(scope, () => {
              patchState(store, setAllEntities([...reviews], reviewConfig));
              store.setFulfilled();
            }),
          ),
          catchError((error: unknown) => failRead(scope, error)),
        );
      }),
    );

    const fetchPage = rxMethod<SearchRequest>(
      switchMap(({ brewerId, page, filters }) => {
        const scope = begin(brewerId);
        return defer(() => api.search(brewerId, page, filters)).pipe(
          tap((result) =>
            whenCurrent(scope, () => {
              store.setPagedResult(result);
              patchState(store, setEntities([...result.results], reviewConfig));
              store.setFulfilled();
            }),
          ),
          catchError((error: unknown) => failRead(scope, error)),
        );
      }),
    );

    const fetchOne = rxMethod<ReviewRequest>(
      switchMap(({ brewerId, reviewId }) => {
        const scope = begin(brewerId);
        return defer(() => api.getById(brewerId, reviewId)).pipe(
          tap((review) =>
            whenCurrent(scope, () => {
              patchState(store, setEntity(review, reviewConfig));
              store.setFulfilled();
            }),
          ),
          catchError((error: unknown) => failRead(scope, error)),
        );
      }),
    );

    /**
     * Builds the request inside the try, so input that makes the API throw still resolves with
     * `error()` set. Applies the response only if the request's scope is still current, and
     * leaves the status alone otherwise.
     */
    async function run<T>(
      brewerId: string,
      request: () => Observable<T>,
      apply: (result: T) => void,
    ): Promise<void> {
      const scope = begin(brewerId);
      try {
        const result = await firstValueFrom(request());
        whenCurrent(scope, () => {
          apply(result);
          store.setFulfilled();
        });
      } catch (error) {
        whenCurrent(scope, () => store.setError(ApiError.from(error)));
      }
    }

    return {
      list(brewerId: string): void {
        fetchList(brewerId);
      },
      search(
        brewerId: string,
        page: BrewerReviewPageQuery,
        filters: BrewerReviewSearchFilters,
      ): void {
        fetchPage({ brewerId, page, filters });
      },
      getById(brewerId: string, reviewId: string): void {
        fetchOne({ brewerId, reviewId });
      },
      create(brewerId: string, review: CreateBrewerReview): Promise<void> {
        return run(
          brewerId,
          () => api.create(brewerId, review),
          (created) => patchState(store, addEntity(created, reviewConfig)),
        );
      },
      delete(brewerId: string, reviewId: string): Promise<void> {
        return run(
          brewerId,
          () => api.delete(brewerId, reviewId),
          () => {
            const reviewKey = normalizeId(reviewId);
            patchState(store, removeEntity(reviewKey), (state) => ({
              results: state.results.filter(
                (existing) => normalizeId(existing.reviewId) !== reviewKey,
              ),
            }));
          },
        );
      },
      /** Forgets the active brewer and everything loaded for it. */
      reset(): void {
        clear(null);
      },
    };
  }),
);
