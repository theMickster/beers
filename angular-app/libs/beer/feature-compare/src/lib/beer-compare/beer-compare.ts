import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  afterNextRender,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  BeerCompareStore,
  CompareSelectionStore,
  MAX_COMPARE_BEERS,
  type Beer,
  type CompareColumn,
} from '@beers/beer/data-access';
import { EmptyStateComponent, RatingComponent } from '@beers/shared/ui';
import { parseCompareParams } from '../compare-params';

/** A comparison needs something to compare against. */
const MIN_COMPARE_BEERS = 2;

interface ColumnView {
  readonly beerId: string;
  readonly beer: Beer | null;
  /** Why there is no beer, or `null` when there is one. */
  readonly unavailable: string | null;
  readonly priceRange: { readonly min: number; readonly max: number } | null;
  readonly styles: string;
}

function toColumnView({ beerId, beer, error }: CompareColumn): ColumnView {
  const prices = (beer?.pricing ?? []).map((tier) => tier.price);
  return {
    beerId,
    beer,
    unavailable:
      beer !== null
        ? null
        : error?.status === 404
          ? 'This beer no longer exists.'
          : 'Could not load this beer.',
    priceRange:
      prices.length === 0
        ? null
        : { min: Math.min(...prices), max: Math.max(...prices) },
    styles: (beer?.beerStyles ?? []).map((style) => style.name).join(', '),
  };
}

@Component({
  selector: 'bw-beer-compare',
  imports: [CurrencyPipe, RouterLink, EmptyStateComponent, RatingComponent],
  templateUrl: './beer-compare.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BeerCompareComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(BeerCompareStore);
  private readonly selection = inject(CompareSelectionStore);

  protected readonly maxCompareBeers = MAX_COMPARE_BEERS;

  /** The URL is the source of truth, so a pasted link renders the same comparison. */
  private readonly params = toSignal(this.route.queryParamMap, {
    requireSync: true,
  });
  protected readonly state = computed(() => parseCompareParams(this.params()));

  // The effect reads these two, not `state`: `state` is a new object on every query emission, so
  // an unrelated query change would otherwise reload the comparison.
  private readonly idsKey = computed(() => this.state().ids.join(','));
  private readonly hasParam = computed(() => this.state().hasParam);

  /**
   * The store is root-scoped and outlives this component, so on a new URL it still holds the
   * previous ids. The columns count as ours only when the store was last asked for these ids.
   */
  protected readonly columns = computed<readonly ColumnView[] | null>(() => {
    const { ids } = this.state();
    const current =
      ids.length >= MIN_COMPARE_BEERS &&
      this.store.ids().join(',') === this.idsKey() &&
      this.store.isFulfilled();
    return current ? this.store.columns().map(toColumnView) : null;
  });

  protected readonly canCompare = computed(
    () => this.state().ids.length >= MIN_COMPARE_BEERS,
  );

  /**
   * The load and the redirect start only in the browser. The server renders the loading state or
   * the prompt, so hydration sees the same markup and the server never calls the API (real data
   * on the server is #1018).
   */
  private readonly browserReady = signal(false);

  constructor() {
    afterNextRender(() => this.browserReady.set(true));

    effect(() => {
      if (!this.browserReady()) return;
      this.idsKey();
      const hasParam = this.hasParam();
      untracked(() => {
        const { ids } = this.state();
        if (ids.length >= MIN_COMPARE_BEERS) {
          this.store.load(ids);
          return;
        }
        // Only a bare `/compare` redirects. A `beers` param that is present but unusable is the
        // user's link, so it shows the prompt instead of being replaced by the selection.
        const selected = this.selection.beerIds();
        if (!hasParam && selected.length >= MIN_COMPARE_BEERS) {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { beers: selected.join(',') },
            queryParamsHandling: 'merge',
            replaceUrl: true,
          });
        }
      });
    });
  }
}
