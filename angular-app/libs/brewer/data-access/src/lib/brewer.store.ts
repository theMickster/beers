import { inject } from '@angular/core';
import { patchState, signalStore, type, withMethods } from '@ngrx/signals';
import {
  addEntity,
  entityConfig,
  removeEntity,
  setAllEntities,
  setEntity,
  withEntities,
} from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  ApiError,
  normalizeId,
  withRequestStatus,
} from '@beers/shared/data-access';
import {
  catchError,
  defer,
  EMPTY,
  firstValueFrom,
  pipe,
  switchMap,
  tap,
} from 'rxjs';
import { BrewerApiService } from './brewer-api.service';
import type {
  Brewer,
  BrewerPatch,
  CreateBrewer,
  UpdateBrewer,
} from './brewer.models';

const brewerConfig = entityConfig({
  entity: type<Brewer>(),
  selectId: (brewer: Brewer) => normalizeId(brewer.brewerId),
});

/**
 * Brewers loaded this session, keyed by lowercase `brewerId`. The server response is
 * authoritative, so every write stores what the API returned rather than what was sent.
 *
 * `requestStatus` is one shared status for reads and writes, so overlapping requests overwrite
 * each other's state. A brewer's `rating` is updated by a change feed and can lag behind a newly
 * created review. Request methods never throw: failures, including input rejected before any
 * request is sent, land in `error()`.
 *
 * A `loadAll()` started before a `delete()` completes can return the deleted brewer and re-add it,
 * so callers should not overlap them.
 */
export const BrewerStore = signalStore(
  { providedIn: 'root' },
  withEntities({ entity: type<Brewer>() }),
  withRequestStatus(),
  withMethods((store) => {
    const api = inject(BrewerApiService);

    const fetchAll = rxMethod<void>(
      pipe(
        tap(() => store.setPending()),
        switchMap(() =>
          defer(() => api.list()).pipe(
            tap((brewers) => {
              patchState(store, setAllEntities([...brewers], brewerConfig));
              store.setFulfilled();
            }),
            catchError((error: unknown) => {
              store.setError(ApiError.from(error));
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    const fetchOne = rxMethod<string>(
      pipe(
        tap(() => store.setPending()),
        switchMap((brewerId) =>
          defer(() => api.getById(brewerId)).pipe(
            tap((brewer) => {
              patchState(store, setEntity(brewer, brewerConfig));
              store.setFulfilled();
            }),
            catchError((error: unknown) => {
              store.setError(ApiError.from(error));
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    async function run(operation: () => Promise<void>): Promise<void> {
      store.setPending();
      try {
        await operation();
        store.setFulfilled();
      } catch (error) {
        store.setError(ApiError.from(error));
      }
    }

    return {
      /** Replaces all entities; no brewers at all (a 404) is an empty, fulfilled store. */
      loadAll(): void {
        fetchAll();
      },
      getById(brewerId: string): void {
        fetchOne(brewerId);
      },
      create(brewer: CreateBrewer): Promise<void> {
        return run(async () => {
          const created = await firstValueFrom(api.create(brewer));
          patchState(store, addEntity(created, brewerConfig));
        });
      },
      /** PUT: full replace. */
      update(brewerId: string, brewer: UpdateBrewer): Promise<void> {
        return run(async () => {
          const updated = await firstValueFrom(api.update(brewerId, brewer));
          patchState(store, setEntity(updated, brewerConfig));
        });
      },
      /** PATCH: partial change of the allow-listed fields. */
      patch(brewerId: string, changes: BrewerPatch): Promise<void> {
        return run(async () => {
          const patched = await firstValueFrom(api.patch(brewerId, changes));
          patchState(store, setEntity(patched, brewerConfig));
        });
      },
      /** A 400 (brewer not deletable) keeps the entity and exposes the message in `error()`. */
      delete(brewerId: string): Promise<void> {
        return run(async () => {
          await firstValueFrom(api.delete(brewerId));
          patchState(store, removeEntity(normalizeId(brewerId)));
        });
      },
    };
  }),
);
