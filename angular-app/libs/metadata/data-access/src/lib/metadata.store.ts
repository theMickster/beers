import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import {
  API_BASE_URL,
  ApiError,
  withRequestStatus,
} from '@beers/shared/data-access';
import {
  catchError,
  firstValueFrom,
  forkJoin,
  of,
  throwError,
  type Observable,
} from 'rxjs';
import type { BeerCategory, BeerStyle, BeerType, BreweryType } from './models';

interface MetadataState {
  beerTypes: readonly BeerType[];
  beerStyles: readonly BeerStyle[];
  beerCategories: readonly BeerCategory[];
  breweryTypes: readonly BreweryType[];
}

const initialState: MetadataState = {
  beerTypes: [],
  beerStyles: [],
  beerCategories: [],
  breweryTypes: [],
};

/**
 * Session-wide cache of the metadata lists. The API answers 404 for an empty list, which is
 * treated as an empty result rather than a failure.
 */
export const MetadataStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withRequestStatus(),
  withMethods((store) => {
    const http = inject(HttpClient);
    const baseUrl = inject(API_BASE_URL);

    const fetchList = <T>(path: string): Observable<readonly T[]> =>
      http
        .get<T[]>(`${baseUrl}/${path}`)
        .pipe(
          catchError((error: unknown) =>
            error instanceof HttpErrorResponse && error.status === 404
              ? of([])
              : throwError(() => error),
          ),
        );

    return {
      /** Loads once per session; no-op while pending or fulfilled. Never rejects. */
      async load(): Promise<void> {
        if (store.isPending() || store.isFulfilled()) return;
        store.setPending();
        try {
          const [beerTypes, beerStyles, beerCategories, breweryTypes] =
            await firstValueFrom(
              forkJoin([
                fetchList<BeerType>('beerTypes'),
                fetchList<BeerStyle>('beerStyles'),
                fetchList<BeerCategory>('beerCategories'),
                fetchList<BreweryType>('breweryTypes'),
              ]),
            );
          patchState(store, {
            beerTypes,
            beerStyles,
            beerCategories,
            breweryTypes,
          });
          store.setFulfilled();
        } catch (error) {
          store.setError(ApiError.from(error));
        }
      },
    };
  }),
);
