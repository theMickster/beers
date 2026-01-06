import { signalStore } from '@ngrx/signals';
import { TestBed } from '@angular/core/testing';
import { ApiError } from '../errors/api-error';
import { withRequestStatus } from './with-request-status';

const TestStore = signalStore({ providedIn: 'root' }, withRequestStatus());

describe('withRequestStatus', () => {
  it('starts idle', () => {
    const store = TestBed.inject(TestStore);
    expect(store.isIdle()).toBe(true);
    expect(store.isPending()).toBe(false);
    expect(store.isFulfilled()).toBe(false);
    expect(store.error()).toBeNull();
  });

  it('transitions to pending then fulfilled', () => {
    const store = TestBed.inject(TestStore);
    store.setPending();
    expect(store.isPending()).toBe(true);
    expect(store.isIdle()).toBe(false);
    store.setFulfilled();
    expect(store.isFulfilled()).toBe(true);
    expect(store.isPending()).toBe(false);
    expect(store.error()).toBeNull();
  });

  it('exposes the error and is neither fulfilled nor pending on failure', () => {
    const store = TestBed.inject(TestStore);
    const error = new ApiError(500, ['boom'], 'server');
    store.setPending();
    store.setError(error);
    expect(store.error()).toBe(error);
    expect(store.isFulfilled()).toBe(false);
    expect(store.isPending()).toBe(false);
    expect(store.isIdle()).toBe(false);
  });

  it('clears the error when a later request is fulfilled', () => {
    const store = TestBed.inject(TestStore);
    store.setError(new ApiError(404, ['missing'], 'client'));
    store.setFulfilled();
    expect(store.error()).toBeNull();
    expect(store.isFulfilled()).toBe(true);
  });
});
