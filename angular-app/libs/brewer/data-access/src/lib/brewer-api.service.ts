import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
} from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_BASE_URL } from '@beers/shared/data-access';
import { catchError, of, throwError, type Observable } from 'rxjs';
import {
  type Brewer,
  type BrewerPatch,
  type BrewerPatchOperation,
  type CreateBrewer,
  type UpdateBrewer,
} from './brewer.models';
import {
  validateBrewerId,
  validateBrewerPatch,
  validateCreateBrewer,
  validateUpdateBrewer,
} from './brewer.validators';
import { whenValid } from './validation';

const JSON_PATCH_HEADERS = new HttpHeaders({
  'Content-Type': 'application/json-patch+json',
});

/**
 * Thin HTTP wrapper over the brewers endpoints. No state, no error mapping.
 *
 * Every write and id-taking method validates first and fails with a client `ApiError` without
 * sending a request when the input breaks a server rule.
 */
@Injectable({ providedIn: 'root' })
export class BrewerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /** Unpaged list. The API answers 404 when there are no brewers, which becomes `[]`. */
  list(): Observable<readonly Brewer[]> {
    return this.http
      .get<Brewer[]>(`${this.baseUrl}/brewers`)
      .pipe(
        catchError((error: unknown) =>
          error instanceof HttpErrorResponse && error.status === 404
            ? of([])
            : throwError(() => error),
        ),
      );
  }

  getById(brewerId: string): Observable<Brewer> {
    return whenValid(validateBrewerId(brewerId), () =>
      this.http.get<Brewer>(this.brewerUrl(brewerId)),
    );
  }

  create(brewer: CreateBrewer): Observable<Brewer> {
    return whenValid(validateCreateBrewer(brewer), () =>
      this.http.post<Brewer>(`${this.baseUrl}/brewers`, toCreateBody(brewer)),
    );
  }

  /** PUT: full replace. A missing brewer is a 400, not a 404. */
  update(brewerId: string, brewer: UpdateBrewer): Observable<Brewer> {
    return whenValid(validateUpdateBrewer(brewerId, brewer), () =>
      this.http.put<Brewer>(this.brewerUrl(brewerId), {
        brewerId,
        ...toCreateBody(brewer),
      }),
    );
  }

  /** PATCH: sends only `changes` as RFC 6902 `replace` operations. */
  patch(brewerId: string, changes: BrewerPatch): Observable<Brewer> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateBrewerPatch(changes),
    ];
    return whenValid(violations, () =>
      this.http.patch<Brewer>(
        this.brewerUrl(brewerId),
        toPatchOperations(changes),
        { headers: JSON_PATCH_HEADERS },
      ),
    );
  }

  delete(brewerId: string): Observable<void> {
    return whenValid(validateBrewerId(brewerId), () =>
      this.http.delete<void>(this.brewerUrl(brewerId)),
    );
  }

  private brewerUrl(brewerId: string): string {
    return `${this.baseUrl}/brewers/${encodeURIComponent(brewerId)}`;
  }
}

/** Picks only the persisted fields, so a spread `Brewer` cannot leak `rating` or `isDeletable`. */
function toCreateBody(brewer: CreateBrewer): CreateBrewer {
  return {
    name: brewer.name,
    foundedIn: brewer.foundedIn,
    headquarters: brewer.headquarters,
    website: brewer.website,
    breweryType: {
      id: brewer.breweryType.id,
      name: brewer.breweryType.name,
    },
  };
}

/** Emits operations in allow-list order for the defined fields only. */
function toPatchOperations(changes: BrewerPatch): BrewerPatchOperation[] {
  const operations: BrewerPatchOperation[] = [];
  if (changes.name !== undefined) {
    operations.push({ op: 'replace', path: '/name', value: changes.name });
  }
  if (changes.foundedIn !== undefined) {
    operations.push({
      op: 'replace',
      path: '/foundedIn',
      value: changes.foundedIn,
    });
  }
  if (changes.headquarters !== undefined) {
    operations.push({
      op: 'replace',
      path: '/headquarters',
      value: changes.headquarters,
    });
  }
  if (changes.website !== undefined) {
    operations.push({
      op: 'replace',
      path: '/website',
      value: changes.website,
    });
  }
  if (changes.breweryType !== undefined) {
    operations.push({
      op: 'replace',
      path: '/breweryType',
      value: { id: changes.breweryType.id, name: changes.breweryType.name },
    });
  }
  return operations;
}
