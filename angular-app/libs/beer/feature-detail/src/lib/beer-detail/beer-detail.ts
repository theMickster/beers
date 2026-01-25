import { CurrencyPipe, DecimalPipe } from '@angular/common';
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
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  BeerDetailStore,
  CompareSelectionStore,
  MAX_COMPARE_BEERS,
} from '@beers/beer/data-access';
import { EmptyStateComponent } from '@beers/shared/ui';

@Component({
  selector: 'bw-beer-detail',
  imports: [CurrencyPipe, DecimalPipe, RouterLink, EmptyStateComponent],
  templateUrl: './beer-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BeerDetailComponent {
  private readonly route = inject(ActivatedRoute);
  protected readonly store = inject(BeerDetailStore);
  protected readonly compare = inject(CompareSelectionStore);

  protected readonly maxCompareBeers = MAX_COMPARE_BEERS;

  private readonly params = toSignal(this.route.paramMap, {
    requireSync: true,
  });
  private readonly beerId = computed(() => this.params().get('beerId') ?? '');

  /** `rating` is nullable and a beer with no reviews carries an average of 0. */
  protected readonly rating = computed(() => {
    const rating = this.store.beer()?.rating;
    return rating && rating.reviewCount > 0 ? rating : null;
  });

  /**
   * The load starts only in the browser. The server renders the loading state, so hydration
   * sees the same markup and the server never calls the API (real data on the server is #1018).
   */
  private readonly browserReady = signal(false);

  constructor() {
    afterNextRender(() => this.browserReady.set(true));

    effect(() => {
      if (!this.browserReady()) return;
      const id = this.beerId();
      untracked(() => this.store.load(id));
    });
  }

  protected retry(): void {
    this.store.load(this.beerId());
  }
}
