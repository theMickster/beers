import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  ApiError,
  normalizeId,
  withRequestStatus,
} from '@beers/shared/data-access';
import { catchError, EMPTY, pipe, switchMap, tap } from 'rxjs';
import { BeerApiService } from './beer-api.service';
import type { Beer } from './models';

/** The API maximum. Seeded brewers have at most 12 beers, so there is no paging. */
const PAGE_SIZE = 50;

interface BeersByBrewerState {
  /** The normalized brewer id last asked for. A component compares it with its route before trusting the rest. */
  readonly brewerId: string | null;
  readonly beers: readonly Beer[];
  readonly totalRecords: number;
}

/**
 * One brewer's beers for the brewer detail page, in a single page of the API maximum. It has its
 * own state so it never touches the explorer's `BeerStore`. `totalRecords` is kept next to the
 * list so a brewer with more beers than fit still shows its real count.
 */
export const BeersByBrewerStore = signalStore(
  { providedIn: 'root' },
  withState<BeersByBrewerState>({ brewerId: null, beers: [], totalRecords: 0 }),
  withRequestStatus(),
  withMethods((store) => {
    const api = inject(BeerApiService);

    const loadBeers = rxMethod<string>(
      pipe(
        switchMap((brewerId) => {
          // Cleared first so the previous brewer's beers are never shown under this one.
          patchState(store, {
            brewerId: normalizeId(brewerId),
            beers: [],
            totalRecords: 0,
          });
          store.setPending();

          return api
            .search({ pageNumber: 1, pageSize: PAGE_SIZE }, { brewerId })
            .pipe(
              tap((result) => {
                patchState(store, {
                  beers: result.results,
                  totalRecords: result.totalRecords,
                });
                store.setFulfilled();
              }),
              catchError((error: unknown) => {
                store.setError(ApiError.from(error));
                return EMPTY;
              }),
            );
        }),
      ),
    );

    return {
      load(brewerId: string): void {
        loadBeers(brewerId);
      },
    };
  }),
);
