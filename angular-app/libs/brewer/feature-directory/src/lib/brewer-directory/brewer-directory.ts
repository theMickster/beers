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
import { BrewerStore } from '@beers/brewer/data-access';
import {
  EmptyStateComponent,
  PaginationComponent,
  RatingComponent,
} from '@beers/shared/ui';
import {
  DIRECTORY_PAGE_SIZE,
  parseDirectoryParams,
  selectDirectoryPage,
} from '../directory-params';

@Component({
  selector: 'bw-brewer-directory',
  imports: [
    RouterLink,
    EmptyStateComponent,
    PaginationComponent,
    RatingComponent,
  ],
  templateUrl: './brewer-directory.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BrewerDirectoryComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly store = inject(BrewerStore);

  protected readonly skeletons = Array.from({ length: DIRECTORY_PAGE_SIZE });

  /** The URL is the source of truth for the filter and the page. */
  private readonly params = toSignal(this.route.queryParamMap, {
    requireSync: true,
  });
  protected readonly view = computed(() =>
    selectDirectoryPage(
      this.store.entities(),
      parseDirectoryParams(this.params()),
    ),
  );

  /**
   * The load starts only in the browser. The server renders the loading state, so hydration sees
   * the same markup and the server never calls the API (real data on the server is #1018). The
   * list also waits for it: a store that outlives the component can already hold brewers from an
   * earlier visit, which the server never had.
   */
  protected readonly browserReady = signal(false);

  constructor() {
    afterNextRender(() => this.browserReady.set(true));

    effect(() => {
      if (!this.browserReady()) return;
      untracked(() => this.store.loadAll());
    });
  }

  protected setType(type: string): void {
    this.navigate({ type: type || null, page: null });
  }

  protected goToPage(page: number): void {
    this.navigate({ page: page > 1 ? page : null });
  }

  protected retry(): void {
    this.store.loadAll();
  }

  /** `null` removes a param; the rest of the query string is kept. */
  private navigate(queryParams: Record<string, unknown>): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge',
    });
  }
}
