import { HttpParams } from '@angular/common/http';
import { isObject, MAX_PAGE_SIZE } from '../validation/validation';

/** The paging inputs the API reads from the query string. */
interface PageQuery {
  readonly pageNumber?: number;
  readonly pageSize?: number;
  readonly sortOrder?: 'asc' | 'desc';
  readonly orderBy?: string;
}

/** Non-finite numbers and a missing `page` are omitted; the server defaults apply. */
export function toPageParams(page: PageQuery): HttpParams {
  let params = new HttpParams();
  if (!isObject(page)) return params;
  if (page.pageNumber !== undefined && Number.isFinite(page.pageNumber)) {
    params = params.set('PageNumber', Math.max(1, Math.trunc(page.pageNumber)));
  }
  if (page.pageSize !== undefined && Number.isFinite(page.pageSize)) {
    params = params.set('PageSize', clampPageSize(page.pageSize));
  }
  if (page.sortOrder !== undefined) {
    params = params.set('SortOrder', page.sortOrder);
  }
  if (page.orderBy !== undefined) {
    params = params.set('OrderBy', page.orderBy);
  }
  return params;
}

/** The server caps at 50 and has no lower bound (zero or less likely fails), so clamp to 1..50. */
function clampPageSize(pageSize: number): number {
  return Math.min(MAX_PAGE_SIZE, Math.max(1, Math.trunc(pageSize)));
}
