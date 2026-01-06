import { computed } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState,
} from '@ngrx/signals';
import type { ApiError } from '../errors/api-error';

export type RequestStatus =
  'idle' | 'pending' | 'fulfilled' | { readonly error: ApiError };

interface RequestStatusState {
  requestStatus: RequestStatus;
}

/**
 * Tracks a request lifecycle. An empty result is `fulfilled`; only a failure is an error.
 */
export function withRequestStatus() {
  return signalStoreFeature(
    withState<RequestStatusState>({ requestStatus: 'idle' }),
    withComputed(({ requestStatus }) => ({
      isIdle: computed(() => requestStatus() === 'idle'),
      isPending: computed(() => requestStatus() === 'pending'),
      isFulfilled: computed(() => requestStatus() === 'fulfilled'),
      error: computed(() => {
        const status = requestStatus();
        return typeof status === 'object' ? status.error : null;
      }),
    })),
    withMethods((store) => ({
      setPending(): void {
        patchState(store, { requestStatus: 'pending' });
      },
      setFulfilled(): void {
        patchState(store, { requestStatus: 'fulfilled' });
      },
      setError(error: ApiError): void {
        patchState(store, { requestStatus: { error } });
      },
    })),
  );
}
