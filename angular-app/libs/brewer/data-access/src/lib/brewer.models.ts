import type { BreweryType } from '@beers/metadata/data-access';

export interface BrewerRating {
  readonly average: number;
  readonly reviewCount: number;
}

/** Mirrors the API's brewer response (camelCase JSON; dates are ISO strings). */
export interface Brewer {
  readonly brewerId: string;
  readonly name: string;
  readonly foundedIn: number;
  readonly headquarters: string;
  readonly website: string;
  readonly isDeletable: boolean;
  readonly breweryType: BreweryType;
  readonly rating: BrewerRating | null;
  readonly createdDate: string;
  readonly modifiedDate: string;
}

/** `isDeletable` and `rating` are server-owned, so they are not part of any write payload. */
export interface CreateBrewer {
  readonly name: string;
  readonly foundedIn: number;
  readonly headquarters: string;
  readonly website: string;
  readonly breweryType: BreweryType;
}

/** `brewerId` must equal the route id; the API rejects a mismatch. */
export type UpdateBrewer = CreateBrewer & { readonly brewerId: string };

/** The only paths the API accepts in a JSON Patch, in the order the service emits them. */
export const BREWER_PATCH_FIELDS = [
  'name',
  'foundedIn',
  'headquarters',
  'website',
  'breweryType',
] as const;

export type BrewerPatchField = (typeof BREWER_PATCH_FIELDS)[number];

/** Partial change set for PATCH. `brewerId`, `isDeletable` and `rating` cannot be patched. */
export type BrewerPatch = Partial<Pick<CreateBrewer, BrewerPatchField>>;

/** RFC 6902 `replace` operation; `breweryType` is always replaced whole, never by sub-path. */
export type BrewerPatchOperation = {
  [Field in BrewerPatchField]: {
    readonly op: 'replace';
    readonly path: `/${Field}`;
    readonly value: CreateBrewer[Field];
  };
}[BrewerPatchField];
