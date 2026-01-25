import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  ApiError,
  isGuid,
  normalizeId,
  withRequestStatus,
} from '@beers/shared/data-access';
import { catchError, EMPTY, pipe, switchMap, tap } from 'rxjs';
import { BeerApiService } from './beer-api.service';
import { BeerStore } from './beer.store';
import type { Beer } from './models';

/**
 * The beer on the detail page. It has its own request status so a failed or missing beer does
 * not disturb the explorer's `BeerStore` state, which it only reads.
 */
export const BeerDetailStore = signalStore(
  { providedIn: 'root' },
  withState<{ beer: Beer | null }>({ beer: null }),
  withRequestStatus(),
  withComputed(({ error }) => ({
    notFound: computed(() => error()?.status === 404),
  })),
  withMethods((store) => {
    const api = inject(BeerApiService);
    const beers = inject(BeerStore);

    const loadBeer = rxMethod<string>(
      pipe(
        switchMap((id) => {
          if (!isGuid(id)) {
            // The API would answer 404 anyway; skip the round trip for ids that cannot exist.
            patchState(store, { beer: null });
            store.setError(new ApiError(404, ['Beer not found.'], 'client'));
            return EMPTY;
          }

          // Show the explorer's copy at once, then replace it with fresh data.
          const cached = beers.entityMap()[normalizeId(id)] ?? null;
          patchState(store, { beer: cached });
          if (cached) {
            store.setFulfilled();
          } else {
            store.setPending();
          }

          return api.getById(id).pipe(
            tap((beer) => {
              patchState(store, { beer });
              store.setFulfilled();
            }),
            // The cached copy goes on any failure, so an error never sits next to a beer. A 404
            // also means the beer was deleted.
            catchError((error: unknown) => {
              patchState(store, { beer: null });
              store.setError(ApiError.from(error));
              return EMPTY;
            }),
          );
        }),
      ),
    );

    return {
      load(id: string): void {
        loadBeer(id);
      },
    };
  }),
);
