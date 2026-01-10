import type {
  BeerCategory,
  BeerStyle,
  BeerType,
} from '@beers/metadata/data-access';

export interface BeerRating {
  readonly average: number;
  readonly reviewCount: number;
}

export interface BeerPrice {
  readonly price: number;
  readonly quantity: number;
  readonly unitVolume: number;
  readonly packaging: string;
}

export interface BrewerSlim {
  readonly id: string;
  readonly name: string;
  readonly website: string;
}

/** Mirrors the API's beer response (camelCase JSON; dates are ISO strings). */
export interface Beer {
  readonly beerId: string;
  readonly brewerId: string;
  readonly name: string;
  readonly description: string;
  readonly image: string;
  readonly sku: string;
  readonly isDeletable: boolean;
  readonly rating: BeerRating | null;
  readonly pricing: readonly BeerPrice[] | null;
  readonly brewer: BrewerSlim;
  readonly beerType: BeerType;
  readonly beerCategories: readonly BeerCategory[];
  readonly beerStyles: readonly BeerStyle[];
  readonly createdDate: string;
  readonly modifiedDate: string;
}

/** The server always sorts by name and clamps `pageSize` to 50 (default 10). */
export interface BeerPageQuery {
  readonly pageNumber?: number;
  readonly pageSize?: number;
  readonly sortOrder?: 'asc' | 'desc';
}

export interface BeerSearchFilters {
  readonly id?: string;
  readonly name?: string;
  readonly brewerId?: string;
  readonly brewerName?: string;
}

export interface CreateBeer {
  readonly brewerId: string;
  readonly name: string;
  readonly description: string;
  readonly image: string;
  readonly sku: string;
  readonly isDeletable: boolean;
  readonly rating?: BeerRating;
  readonly pricing?: readonly BeerPrice[];
  readonly brewer: BrewerSlim;
  readonly beerTypeId: string;
  readonly beerCategories: readonly string[];
  readonly beerStyles: readonly string[];
}

/** `beerId` must equal the route id; the API rejects a mismatch. */
export type UpdateBeer = CreateBeer & { readonly beerId: string };
