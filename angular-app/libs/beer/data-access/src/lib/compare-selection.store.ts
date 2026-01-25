import { computed } from '@angular/core';
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from '@ngrx/signals';
import { normalizeId } from '@beers/shared/data-access';

/** The compare story caps a comparison at 4 beers. */
export const MAX_COMPARE_BEERS = 4;

/**
 * Beers the user picked for comparison, in the order they were picked. In-memory only: a full
 * page load starts empty.
 */
export const CompareSelectionStore = signalStore(
  { providedIn: 'root' },
  withState<{ beerIds: readonly string[] }>({ beerIds: [] }),
  withComputed(({ beerIds }) => ({
    isFull: computed(() => beerIds().length >= MAX_COMPARE_BEERS),
  })),
  withMethods((store) => ({
    /** Reads the signal, so calling it inside a `computed` or template stays reactive. */
    isSelected(id: string): boolean {
      return store.beerIds().includes(normalizeId(id));
    },
    /** Removes a selected beer. Adds an unselected one unless the selection is full. */
    toggle(id: string): void {
      const key = normalizeId(id);
      patchState(store, ({ beerIds }) => {
        if (beerIds.includes(key)) {
          return { beerIds: beerIds.filter((existing) => existing !== key) };
        }
        return beerIds.length >= MAX_COMPARE_BEERS
          ? { beerIds }
          : { beerIds: [...beerIds, key] };
      });
    },
  })),
);
