import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';
import type { PagedResult } from '../models/paged-result';
import { withPagination } from './with-pagination';

interface Beer {
  id: string;
  name: string;
}
interface Brewer {
  id: string;
  city: string;
}

const BeerStore = signalStore({ providedIn: 'root' }, withPagination<Beer>());
const BrewerStore = signalStore(
  { providedIn: 'root' },
  withPagination<Brewer>(),
);

describe('withPagination', () => {
  it('starts with an empty first page', () => {
    const store = TestBed.inject(BeerStore);
    expect(store.results()).toEqual([]);
    expect(store.pageNumber()).toBe(1);
    expect(store.totalRecords()).toBe(0);
    expect(store.hasNextPage()).toBe(false);
    expect(store.hasPreviousPage()).toBe(false);
  });

  it('patches every field from a paged result', () => {
    const store = TestBed.inject(BeerStore);
    const result: PagedResult<Beer> = {
      pageNumber: 2,
      pageSize: 2,
      totalPages: 3,
      hasPreviousPage: true,
      hasNextPage: true,
      totalRecords: 6,
      results: [
        { id: '1', name: 'Pils' },
        { id: '2', name: 'Stout' },
      ],
    };
    store.setPagedResult(result);
    expect(store.results()).toEqual(result.results);
    expect(store.pageNumber()).toBe(2);
    expect(store.pageSize()).toBe(2);
    expect(store.totalPages()).toBe(3);
    expect(store.totalRecords()).toBe(6);
    expect(store.hasPreviousPage()).toBe(true);
    expect(store.hasNextPage()).toBe(true);
  });

  it('works for a different entity type', () => {
    const store = TestBed.inject(BrewerStore);
    store.setPagedResult({
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: 1,
      results: [{ id: 'b1', city: 'Denver' }],
    });
    expect(store.results()[0]?.city).toBe('Denver');
  });

  it('replaces prior results with an empty page', () => {
    const store = TestBed.inject(BeerStore);
    store.setPagedResult({
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: 1,
      results: [{ id: '1', name: 'Pils' }],
    });
    store.setPagedResult({
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: 0,
      results: [],
    });
    expect(store.results()).toEqual([]);
    expect(store.totalRecords()).toBe(0);
    expect(store.totalPages()).toBe(0);
  });
});
