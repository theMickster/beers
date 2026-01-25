import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { ApiError, withRequestStatus } from '@beers/shared/data-access';
import {
  EMPTY,
  catchError,
  forkJoin,
  map,
  of,
  pipe,
  switchMap,
  tap,
} from 'rxjs';
import { BeerApiService } from './beer-api.service';
import type { Beer } from './models';

/** One beer in a comparison. A beer that could not be fetched keeps its slot and carries why. */
export interface CompareColumn {
  readonly beerId: string;
  readonly beer: Beer | null;
  readonly error: ApiError | null;
}

interface BeerCompareState {
  /** The ids last asked for. A component compares it with its route to know the columns are current. */
  readonly ids: readonly string[];
  readonly columns: readonly CompareColumn[];
}

/**
 * The beers on the compare page. The API has no by-ids endpoint and the explorer's cache is
 * replaced on every search, so each beer is fetched by id. One beer failing does not fail the
 * comparison: its failure is data in its column, so the store's own error is never set.
 */
export const BeerCompareStore = signalStore(
  { providedIn: 'root' },
  withState<BeerCompareState>({ ids: [], columns: [] }),
  withRequestStatus(),
  withMethods((store) => {
    const api = inject(BeerApiService);

    const loadColumns = rxMethod<readonly string[]>(
      pipe(
        switchMap((ids) => {
          // Cleared first so a previous comparison is never shown under the new ids.
          patchState(store, { ids, columns: [] });
          if (ids.length === 0) {
            patchState(store, { requestStatus: 'idle' });
            return EMPTY;
          }
          store.setPending();

          return forkJoin(
            ids.map((beerId) =>
              api.getById(beerId).pipe(
                map((beer): CompareColumn => ({ beerId, beer, error: null })),
                catchError((error: unknown) =>
                  of<CompareColumn>({
                    beerId,
                    beer: null,
                    error: ApiError.from(error),
                  }),
                ),
              ),
            ),
          ).pipe(
            tap((columns) => {
              patchState(store, { columns });
              store.setFulfilled();
            }),
          );
        }),
      ),
    );

    return {
      load(ids: readonly string[]): void {
        loadColumns(ids);
      },
    };
  }),
);
