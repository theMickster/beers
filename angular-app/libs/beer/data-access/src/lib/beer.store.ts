import { inject } from '@angular/core';
import { patchState, signalStore, type, withMethods } from '@ngrx/signals';
import {
  entityConfig,
  removeEntity,
  setEntities,
  setEntity,
  addEntity,
  withEntities,
} from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  ApiError,
  withPagination,
  withRequestStatus,
} from '@beers/shared/data-access';
import { catchError, EMPTY, firstValueFrom, pipe, switchMap, tap } from 'rxjs';
import { BeerApiService } from './beer-api.service';
import type {
  Beer,
  BeerPageQuery,
  BeerSearchFilters,
  CreateBeer,
  UpdateBeer,
} from './models';

const beerConfig = entityConfig({
  entity: type<Beer>(),
  selectId: (beer: Beer) => beer.beerId,
});

interface PageRequest {
  readonly page: BeerPageQuery;
  readonly filters: BeerSearchFilters;
}

/**
 * Beers loaded this session, keyed by `beerId`, plus the current search page.
 *
 * create/update/delete keep entities and the current `results` in sync but do NOT adjust
 * `totalRecords` or `totalPages`; callers should refetch the page when those matter.
 * Request methods never throw: failures land in `error()`.
 */
export const BeerStore = signalStore(
  { providedIn: 'root' },
  withEntities({ entity: type<Beer>() }),
  withPagination<Beer>(),
  withRequestStatus(),
  withMethods((store) => {
    const api = inject(BeerApiService);

    const fetchPage = rxMethod<PageRequest>(
      pipe(
        tap(() => store.setPending()),
        switchMap(({ page, filters }) =>
          api.search(page, filters).pipe(
            tap((result) => {
              store.setPagedResult(result);
              patchState(store, setEntities([...result.results], beerConfig));
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
        switchMap((id) =>
          api.getById(id).pipe(
            tap((beer) => {
              patchState(store, setEntity(beer, beerConfig));
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
      loadPage(page: BeerPageQuery): void {
        fetchPage({ page, filters: {} });
      },
      search(request: PageRequest): void {
        fetchPage(request);
      },
      getById(id: string): void {
        fetchOne(id);
      },
      create(beer: CreateBeer): Promise<void> {
        return run(async () => {
          const created = await firstValueFrom(api.create(beer));
          patchState(store, addEntity(created, beerConfig));
        });
      },
      update(beer: UpdateBeer): Promise<void> {
        return run(async () => {
          const updated = await firstValueFrom(api.update(beer));
          patchState(store, setEntity(updated, beerConfig), (state) => ({
            results: state.results.map((existing) =>
              existing.beerId === updated.beerId ? updated : existing,
            ),
          }));
        });
      },
      delete(id: string): Promise<void> {
        return run(async () => {
          await firstValueFrom(api.delete(id));
          patchState(store, removeEntity(id), (state) => ({
            results: state.results.filter((existing) => existing.beerId !== id),
          }));
        });
      },
    };
  }),
);
