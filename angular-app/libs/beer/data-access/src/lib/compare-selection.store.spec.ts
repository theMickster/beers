import { TestBed } from '@angular/core/testing';
import {
  CompareSelectionStore,
  MAX_COMPARE_BEERS,
} from './compare-selection.store';

const ids = Array.from(
  { length: MAX_COMPARE_BEERS + 1 },
  (_, i) => `3f2504e0-4f89-11d3-9a0c-0305e82c330${i}`,
);

describe('CompareSelectionStore', () => {
  let store: InstanceType<typeof CompareSelectionStore>;

  beforeEach(() => {
    store = TestBed.inject(CompareSelectionStore);
  });

  function fill(count: number): void {
    ids.slice(0, count).forEach((id) => store.toggle(id));
  }

  it('selects a beer on the first toggle and deselects it on the second', () => {
    store.toggle(ids[0]);
    expect(store.isSelected(ids[0])).toBe(true);

    store.toggle(ids[0]);
    expect(store.isSelected(ids[0])).toBe(false);
    expect(store.beerIds()).toEqual([]);
  });

  it('keeps beers in the order they were selected', () => {
    store.toggle(ids[2]);
    store.toggle(ids[0]);
    store.toggle(ids[1]);

    store.toggle(ids[0]);
    store.toggle(ids[0]);

    expect(store.beerIds()).toEqual([ids[2], ids[1], ids[0]]);
  });

  it('treats ids that differ only by case as the same beer', () => {
    store.toggle(ids[0].toUpperCase());

    expect(store.isSelected(ids[0])).toBe(true);
    expect(store.isSelected(ids[0].toUpperCase())).toBe(true);
    store.toggle(ids[0]);
    expect(store.beerIds()).toEqual([]);
  });

  it('becomes full at the cap and not before', () => {
    fill(MAX_COMPARE_BEERS - 1);
    expect(store.isFull()).toBe(false);

    store.toggle(ids[MAX_COMPARE_BEERS - 1]);
    expect(store.isFull()).toBe(true);
  });

  it('ignores a new beer when full, leaving the selection unchanged', () => {
    fill(MAX_COMPARE_BEERS);

    store.toggle(ids[MAX_COMPARE_BEERS]);

    expect(store.isSelected(ids[MAX_COMPARE_BEERS])).toBe(false);
    expect(store.beerIds()).toEqual(ids.slice(0, MAX_COMPARE_BEERS));
  });

  it('still removes a selected beer when full, which frees a slot', () => {
    fill(MAX_COMPARE_BEERS);

    store.toggle(ids[1]);
    expect(store.isFull()).toBe(false);

    store.toggle(ids[MAX_COMPARE_BEERS]);
    expect(store.isSelected(ids[MAX_COMPARE_BEERS])).toBe(true);
  });
});
