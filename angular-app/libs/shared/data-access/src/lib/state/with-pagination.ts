import {
  patchState,
  signalStoreFeature,
  withMethods,
  withState,
} from '@ngrx/signals';
import type { PagedResult } from '../models/paged-result';

export interface PaginationState<Entity> {
  results: Entity[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/** Holds the current page of `Entity` plus paging metadata from the API. */
export function withPagination<Entity>() {
  return signalStoreFeature(
    withState<PaginationState<Entity>>({
      results: [],
      pageNumber: 1,
      pageSize: 0,
      totalPages: 0,
      totalRecords: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    }),
    withMethods((store) => ({
      setPagedResult(result: PagedResult<Entity>): void {
        patchState(store, {
          results: [...result.results],
          pageNumber: result.pageNumber,
          pageSize: result.pageSize,
          totalPages: result.totalPages,
          totalRecords: result.totalRecords,
          hasPreviousPage: result.hasPreviousPage,
          hasNextPage: result.hasNextPage,
        });
      },
    })),
  );
}
