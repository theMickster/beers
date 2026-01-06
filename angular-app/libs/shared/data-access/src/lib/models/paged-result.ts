/** Mirrors the API's `SearchResultBaseModel<T>` (camelCase JSON). */
export interface PagedResult<T> {
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasPreviousPage: boolean;
  readonly hasNextPage: boolean;
  readonly totalRecords: number;
  readonly results: readonly T[];
}
