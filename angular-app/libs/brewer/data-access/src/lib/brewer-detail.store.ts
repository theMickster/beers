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
import { BrewerApiService } from './brewer-api.service';
import type { Brewer } from './brewer.models';
import { BrewerStore } from './brewer.store';

interface BrewerDetailState {
  /** The normalized id last asked for. A component compares it with its route before trusting the rest. */
  readonly brewerId: string | null;
  readonly brewer: Brewer | null;
}

/**
 * The brewer on the detail page. It has its own request status so a failed or missing brewer does
 * not disturb the directory's `BrewerStore` state, which it only reads.
 */
export const BrewerDetailStore = signalStore(
  { providedIn: 'root' },
  withState<BrewerDetailState>({ brewerId: null, brewer: null }),
  withRequestStatus(),
  withComputed(({ error }) => ({
    notFound: computed(() => error()?.status === 404),
  })),
  withMethods((store) => {
    const api = inject(BrewerApiService);
    const brewers = inject(BrewerStore);

    const loadBrewer = rxMethod<string>(
      pipe(
        switchMap((id) => {
          const brewerId = normalizeId(id);
          if (!isGuid(id)) {
            // The API would answer 404 anyway; skip the round trip for ids that cannot exist.
            patchState(store, { brewerId, brewer: null });
            store.setError(new ApiError(404, ['Brewer not found.'], 'client'));
            return EMPTY;
          }

          // Show the directory's copy at once, then replace it with fresh data.
          const cached = brewers.entityMap()[brewerId] ?? null;
          patchState(store, { brewerId, brewer: cached });
          if (cached) {
            store.setFulfilled();
          } else {
            store.setPending();
          }

          return api.getById(id).pipe(
            tap((brewer) => {
              patchState(store, { brewer });
              store.setFulfilled();
            }),
            // The cached copy goes on any failure, so an error never sits next to a brewer. A 404
            // also means the brewer was deleted.
            catchError((error: unknown) => {
              patchState(store, { brewer: null });
              store.setError(ApiError.from(error));
              return EMPTY;
            }),
          );
        }),
      ),
    );

    return {
      load(id: string): void {
        loadBrewer(id);
      },
    };
  }),
);
