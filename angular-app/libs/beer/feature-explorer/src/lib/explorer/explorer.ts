import {
  ChangeDetectionStrategy,
  Component,
  afterNextRender,
  computed,
  effect,
  inject,
  linkedSignal,
  signal,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { BeerStore } from '@beers/beer/data-access';
import { MetadataStore } from '@beers/metadata/data-access';
import { EmptyStateComponent } from '@beers/shared/ui';
import { BeerCardComponent } from '../beer-card/beer-card';
import {
  DEFAULT_PAGE_SIZE,
  PAGE_SIZE_OPTIONS,
  pageWindow,
  parseExplorerParams,
  parsePrice,
  type FacetKey,
} from './explorer-params';

const SKELETON_COUNT = 6;

@Component({
  selector: 'bw-explorer',
  imports: [BeerCardComponent, EmptyStateComponent],
  templateUrl: './explorer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExplorerComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly metadata = inject(MetadataStore);
  protected readonly store = inject(BeerStore);

  protected readonly pageSizeOptions = PAGE_SIZE_OPTIONS;
  protected readonly skeletons = Array.from({ length: SKELETON_COUNT });

  /** The URL is the source of truth for filters and paging. */
  private readonly params = toSignal(this.route.queryParamMap, {
    requireSync: true,
  });
  protected readonly state = computed(() => parseExplorerParams(this.params()));

  private readonly request = computed(() => {
    const {
      page,
      pageSize,
      styleIds,
      categoryIds,
      typeIds,
      minPrice,
      maxPrice,
    } = this.state();
    return {
      page: { pageNumber: page, pageSize },
      filters: {
        beerStyleIds: styleIds,
        beerCategoryIds: categoryIds,
        beerTypeIds: typeIds,
        minPrice,
        maxPrice,
      },
    };
  });

  protected readonly facets = computed(() => {
    const { styleIds, categoryIds, typeIds } = this.state();
    return [
      {
        key: 'style' as const,
        label: 'Style',
        options: this.metadata.beerStyles(),
        selected: styleIds,
      },
      {
        key: 'category' as const,
        label: 'Category',
        options: this.metadata.beerCategories(),
        selected: categoryIds,
      },
      {
        key: 'type' as const,
        label: 'Type',
        options: this.metadata.beerTypes(),
        selected: typeIds,
      },
    ];
  });

  protected readonly minPriceInput = linkedSignal(
    () => this.state().minPrice?.toString() ?? '',
  );
  protected readonly maxPriceInput = linkedSignal(
    () => this.state().maxPrice?.toString() ?? '',
  );

  /** Why the typed price range cannot be applied, or `null` when it can. */
  protected readonly priceError = computed(() => {
    const min = this.minPriceInput().trim();
    const max = this.maxPriceInput().trim();
    const minPrice = parsePrice(min);
    const maxPrice = parsePrice(max);
    if (
      (min !== '' && minPrice === undefined) ||
      (max !== '' && maxPrice === undefined)
    ) {
      return 'Enter prices as numbers of 0 or more.';
    }
    if (
      minPrice !== undefined &&
      maxPrice !== undefined &&
      minPrice > maxPrice
    ) {
      return 'Minimum price cannot be greater than maximum price.';
    }
    return null;
  });

  protected readonly hasFilters = computed(() => {
    const { styleIds, categoryIds, typeIds, minPrice, maxPrice } = this.state();
    return (
      styleIds.length + categoryIds.length + typeIds.length > 0 ||
      minPrice !== undefined ||
      maxPrice !== undefined
    );
  });

  /** The metadata store reports a failed load through `error()`; the filter lists stay empty. */
  protected readonly metadataFailed = computed(
    () => this.metadata.error() !== null,
  );

  protected readonly pages = computed(() =>
    pageWindow(this.store.pageNumber(), this.store.totalPages()),
  );

  protected readonly showSkeleton = computed(
    () =>
      this.store.isIdle() ||
      (this.store.isPending() && this.store.results().length === 0),
  );

  /**
   * The search starts only in the browser. The server renders the loading state, so hydration
   * sees the same markup and the server never calls the API (real data on the server is #1018).
   */
  private readonly browserReady = signal(false);

  constructor() {
    afterNextRender(() => {
      void this.metadata.load();
      this.browserReady.set(true);
    });

    effect(() => {
      if (!this.browserReady()) return;
      const request = this.request();
      untracked(() => this.store.search(request));
    });
  }

  protected toggleFacet(key: FacetKey, id: string): void {
    const facet = this.facets().find((f) => f.key === key);
    const selected = facet?.selected ?? [];
    const next = selected.includes(id)
      ? selected.filter((existing) => existing !== id)
      : [...selected, id];
    this.navigate({ [key]: next.length > 0 ? next : null, page: null });
  }

  protected applyPrice(event: Event): void {
    event.preventDefault();
    if (this.priceError() !== null) return;
    this.navigate({
      minPrice: parsePrice(this.minPriceInput()) ?? null,
      maxPrice: parsePrice(this.maxPriceInput()) ?? null,
      page: null,
    });
  }

  protected clearFilters(): void {
    this.navigate({
      style: null,
      category: null,
      type: null,
      minPrice: null,
      maxPrice: null,
      page: null,
    });
  }

  protected goToPage(page: number): void {
    this.navigate({ page: page > 1 ? page : null });
  }

  protected setPageSize(pageSize: number): void {
    this.navigate({
      pageSize: pageSize === DEFAULT_PAGE_SIZE ? null : pageSize,
      page: null,
    });
  }

  protected retry(): void {
    this.store.search(this.request());
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
