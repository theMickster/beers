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
import { BlogApiService } from './blog-api.service';
import type {
  BlogPostPageQuery,
  BlogPostSearchFilters,
  CreateNewsBlogPost,
  NewsBlogPost,
  UpdateNewsBlogPost,
} from './blog.models';

const postConfig = entityConfig({
  entity: type<NewsBlogPost>(),
  selectId: (post: NewsBlogPost) => normalizeId(post.newsBlogPostId),
});

/** The scope of `search`. A GUID can never equal it. */
const ALL_SCOPE = 'all';

const EMPTY_PAGE: PaginationState<NewsBlogPost> = {
  results: [],
  pageNumber: 1,
  pageSize: 0,
  totalPages: 0,
  totalRecords: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

interface SearchRequest {
  readonly page: BlogPostPageQuery;
  readonly filters: BlogPostSearchFilters;
}

interface PostRequest {
  readonly brewerId: string;
  readonly postId: string;
}

/** Identifies the scope and the clear-epoch a request started under. */
interface RequestScope {
  readonly key: string;
  readonly epoch: number;
}

/**
 * Blog posts for one scope at a time: either one brewer (the reads that take a `brewerId`) or
 * the cross-brewer feed (`search`). Using a different scope than the last call clears entities,
 * the current page and the status first. Ids compare case-insensitively and are stored
 * lowercase. A `brewerId` inside the search filters does not change the scope; it is still the
 * feed. Responses that arrive after the store was cleared (a scope switch or `reset()`) are
 * ignored, even when the same scope is active again, so one scope's posts never appear under
 * another, an older request cannot overwrite a newer one's state, and a stale response cannot
 * change `requestStatus`.
 *
 * `listByBrewer` replaces entities only and leaves the paged `results` alone. `search` fills
 * `results`, paging metadata and entities. `getById` uses the brewer scope, so opening a post
 * from the feed clears the feed. create/update/delete keep entities in sync, and update/delete
 * also keep `results` in sync, but they do NOT adjust `totalRecords` or `totalPages`, and create
 * never adds to `results`; callers should refetch the page when those matter. While the feed is
 * active, writes stay in the feed scope whatever their `brewerId`; otherwise they use the brewer
 * scope like the reads. `requestStatus` is one shared status for reads and writes, and request
 * methods never throw: failures land in `error()`.
 *
 * When the scope changed before a write completed, its result or error is dropped and the promise
 * still resolves; a successful write still changed the server.
 */
export const BlogStore = signalStore(
  { providedIn: 'root' },
  withState<{ activeScope: string | null }>({ activeScope: null }),
  withEntities({ entity: type<NewsBlogPost>() }),
  withPagination<NewsBlogPost>(),
  withRequestStatus(),
  withMethods((store) => {
    const api = inject(BlogApiService);
    let epoch = 0;

    function clear(activeScope: string | null): void {
      epoch++;
      patchState(
        store,
        { activeScope, requestStatus: 'idle' as const, ...EMPTY_PAGE },
        removeAllEntities(),
      );
    }

    /** Marks the request as pending for `key`, switching scopes first if needed. */
    function begin(key: string): RequestScope {
      if (store.activeScope() !== key) {
        clear(key);
      }
      store.setPending();
      return { key, epoch };
    }

    function whenCurrent(scope: RequestScope, action: () => void): void {
      if (store.activeScope() === scope.key && epoch === scope.epoch) {
        action();
      }
    }

    function failRead(scope: RequestScope, error: unknown): typeof EMPTY {
      whenCurrent(scope, () => store.setError(ApiError.from(error)));
      return EMPTY;
    }

    const fetchList = rxMethod<string>(
      switchMap((brewerId) => {
        const scope = begin(normalizeId(brewerId));
        return defer(() => api.listByBrewer(brewerId)).pipe(
          tap((posts) =>
            whenCurrent(scope, () => {
              patchState(store, setAllEntities([...posts], postConfig));
              store.setFulfilled();
            }),
          ),
          catchError((error: unknown) => failRead(scope, error)),
        );
      }),
    );

    const fetchPage = rxMethod<SearchRequest>(
      switchMap(({ page, filters }) => {
        const scope = begin(ALL_SCOPE);
        return defer(() => api.search(page, filters)).pipe(
          tap((result) =>
            whenCurrent(scope, () => {
              store.setPagedResult(result);
              patchState(store, setEntities([...result.results], postConfig));
              store.setFulfilled();
            }),
          ),
          catchError((error: unknown) => failRead(scope, error)),
        );
      }),
    );

    const fetchOne = rxMethod<PostRequest>(
      switchMap(({ brewerId, postId }) => {
        const scope = begin(normalizeId(brewerId));
        return defer(() => api.getById(brewerId, postId)).pipe(
          tap((post) =>
            whenCurrent(scope, () => {
              patchState(store, setEntity(post, postConfig));
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
     * leaves the status alone otherwise. While the feed is active a write stays in the feed
     * scope, so an edit or delete changes the loaded page in place instead of clearing it.
     */
    async function run<T>(
      brewerId: string,
      request: () => Observable<T>,
      apply: (result: T) => void,
    ): Promise<void> {
      const scope = begin(
        store.activeScope() === ALL_SCOPE ? ALL_SCOPE : normalizeId(brewerId),
      );
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
      listByBrewer(brewerId: string): void {
        fetchList(brewerId);
      },
      search(page: BlogPostPageQuery, filters: BlogPostSearchFilters): void {
        fetchPage({ page, filters });
      },
      getById(brewerId: string, postId: string): void {
        fetchOne({ brewerId, postId });
      },
      create(brewerId: string, post: CreateNewsBlogPost): Promise<void> {
        return run(
          brewerId,
          () => api.create(brewerId, post),
          (created) => patchState(store, addEntity(created, postConfig)),
        );
      },
      update(
        brewerId: string,
        postId: string,
        post: UpdateNewsBlogPost,
      ): Promise<void> {
        return run(
          brewerId,
          () => api.update(brewerId, postId, post),
          (updated) => {
            const postKey = normalizeId(updated.newsBlogPostId);
            patchState(store, setEntity(updated, postConfig), (state) => ({
              results: state.results.map((existing) =>
                normalizeId(existing.newsBlogPostId) === postKey
                  ? updated
                  : existing,
              ),
            }));
          },
        );
      },
      delete(brewerId: string, postId: string): Promise<void> {
        return run(
          brewerId,
          () => api.delete(brewerId, postId),
          () => {
            const postKey = normalizeId(postId);
            patchState(store, removeEntity(postKey), (state) => ({
              results: state.results.filter(
                (existing) => normalizeId(existing.newsBlogPostId) !== postKey,
              ),
            }));
          },
        );
      },
      /** Forgets the active scope and everything loaded for it. */
      reset(): void {
        clear(null);
      },
    };
  }),
);
