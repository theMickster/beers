import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
} from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_BASE_URL, type PagedResult } from '@beers/shared/data-access';
import { catchError, of, throwError, type Observable } from 'rxjs';
import type {
  Beer,
  BeerPageQuery,
  BeerSearchFilters,
  CreateBeer,
  UpdateBeer,
} from './models';

const FILTER_KEYS = ['id', 'name', 'brewerId', 'brewerName'] as const;

/** Thin HTTP wrapper over the beers endpoints. No state, no error mapping. */
@Injectable({ providedIn: 'root' })
export class BeerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /** Unpaged list. The API answers 404 when there are no beers, which becomes `[]`. */
  list(): Observable<readonly Beer[]> {
    return this.http
      .get<Beer[]>(`${this.baseUrl}/beers`)
      .pipe(
        catchError((error: unknown) =>
          error instanceof HttpErrorResponse && error.status === 404
            ? of([])
            : throwError(() => error),
        ),
      );
  }

  search(
    page: BeerPageQuery,
    filters: BeerSearchFilters,
  ): Observable<PagedResult<Beer>> {
    return this.http.post<PagedResult<Beer>>(
      `${this.baseUrl}/beers/search`,
      toSearchBody(filters),
      { params: toPageParams(page) },
    );
  }

  getById(beerId: string): Observable<Beer> {
    return this.http.get<Beer>(this.beerUrl(beerId));
  }

  create(beer: CreateBeer): Observable<Beer> {
    return this.http.post<Beer>(`${this.baseUrl}/beers`, beer);
  }

  update(beer: UpdateBeer): Observable<Beer> {
    return this.http.put<Beer>(this.beerUrl(beer.beerId), beer);
  }

  delete(beerId: string): Observable<void> {
    return this.http.delete<void>(this.beerUrl(beerId));
  }

  private beerUrl(beerId: string): string {
    return `${this.baseUrl}/beers/${encodeURIComponent(beerId)}`;
  }
}

function toPageParams(page: BeerPageQuery): HttpParams {
  let params = new HttpParams();
  if (page.pageNumber !== undefined) {
    params = params.set('PageNumber', page.pageNumber);
  }
  if (page.pageSize !== undefined) {
    params = params.set('PageSize', page.pageSize);
  }
  if (page.sortOrder !== undefined) {
    params = params.set('SortOrder', page.sortOrder);
  }
  return params;
}

/** The API requires a body, so an empty filter set still sends `{}`. */
function toSearchBody(filters: BeerSearchFilters): BeerSearchFilters {
  const body: Record<string, string> = {};
  for (const key of FILTER_KEYS) {
    const value = filters[key];
    if (value !== undefined && value.trim() !== '') {
      body[key] = value;
    }
  }
  return body;
}
