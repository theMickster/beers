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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BeersByBrewerStore } from '@beers/beer/data-access';
import {
  BrewerDetailStore,
  BrewerReviewStore,
  MAX_RATING,
  MAX_REVIEW_COMMENTS_LENGTH,
  MAX_REVIEW_TITLE_LENGTH,
  MAX_REVIEWER_NAME_LENGTH,
  MIN_RATING,
  validateCreateBrewerReview,
} from '@beers/brewer/data-access';
import { isGuid, normalizeId } from '@beers/shared/data-access';
import {
  EmptyStateComponent,
  PaginationComponent,
  RatingComponent,
} from '@beers/shared/ui';

const REVIEWS_PAGE_SIZE = 10;
const POSITIVE_INTEGER = /^\d+$/;

// Not `DatePipe`: it adds about 9 kB of `@angular/common` date code to the initial bundle.
// UTC and a fixed locale keep the server and the browser rendering the same text.
const REVIEW_DATE = new Intl.DateTimeFormat('en-US', {
  dateStyle: 'medium',
  timeZone: 'UTC',
});

/** A positive integer from the URL, else 1. The URL is user-editable. */
function parsePage(value: string | null): number {
  return POSITIVE_INTEGER.test(value ?? '') ? Math.max(Number(value), 1) : 1;
}

@Component({
  selector: 'bw-brewer-detail',
  imports: [
    RouterLink,
    EmptyStateComponent,
    PaginationComponent,
    RatingComponent,
  ],
  templateUrl: './brewer-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BrewerDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly detail = inject(BrewerDetailStore);
  protected readonly beers = inject(BeersByBrewerStore);
  protected readonly reviews = inject(BrewerReviewStore);

  protected readonly maxReviewerNameLength = MAX_REVIEWER_NAME_LENGTH;
  protected readonly maxReviewTitleLength = MAX_REVIEW_TITLE_LENGTH;
  protected readonly maxReviewCommentsLength = MAX_REVIEW_COMMENTS_LENGTH;
  protected readonly maxRating = MAX_RATING;
  protected readonly ratingOptions = Array.from(
    { length: MAX_RATING - MIN_RATING + 1 },
    (_, i) => MIN_RATING + i,
  );
  protected readonly skeletons = Array.from({ length: 3 });

  private readonly params = toSignal(this.route.paramMap, {
    requireSync: true,
  });
  private readonly query = toSignal(this.route.queryParamMap, {
    requireSync: true,
  });
  /** Stores key their data by the lowercase id, so the route is compared in that form. */
  private readonly brewerId = computed(() =>
    normalizeId(this.params().get('brewerId') ?? ''),
  );
  protected readonly page = computed(() => parsePage(this.query().get('page')));

  // These stores are root-scoped and outlive the component, and the load starts only after the
  // first render. Each section therefore trusts its store only once the store says it was last
  // asked for this route's brewer; otherwise it shows loading, never another brewer's data.
  protected readonly brewer = computed(() =>
    this.detail.brewerId() === this.brewerId() ? this.detail.brewer() : null,
  );
  protected readonly notFound = computed(
    () => this.detail.brewerId() === this.brewerId() && this.detail.notFound(),
  );
  protected readonly error = computed(() =>
    this.detail.brewerId() === this.brewerId() ? this.detail.error() : null,
  );
  protected readonly beersLoaded = computed(
    () => this.beers.brewerId() === this.brewerId() && this.beers.isFulfilled(),
  );
  protected readonly beersError = computed(() =>
    this.beers.brewerId() === this.brewerId() ? this.beers.error() : null,
  );
  private readonly reviewsCurrent = computed(
    () => this.reviews.activeBrewerId() === this.brewerId(),
  );
  /** Set while a review is being created, so the list stays on screen instead of blanking. */
  protected readonly submitting = signal(false);
  // A failed create leaves its error in the shared status until the search that follows it.
  protected readonly reviewsError = computed(() =>
    this.reviewsCurrent() && !this.submitting() ? this.reviews.error() : null,
  );
  protected readonly reviewsLoading = computed(
    () =>
      !this.reviewsCurrent() ||
      (this.reviews.isPending() && !this.submitting()),
  );

  protected readonly reviewerName = signal('');
  protected readonly title = signal('');
  protected readonly comments = signal('');
  protected readonly rating = signal<number | null>(null);
  protected readonly attempted = signal(false);
  protected readonly violations = computed(() =>
    validateCreateBrewerReview({
      reviewerName: this.reviewerName(),
      title: this.title(),
      comments: this.comments(),
      // 0 fails the rating rule, which is what an unchosen rating should do.
      rating: this.rating() ?? 0,
    }),
  );
  // A message about a review of one brewer must not stay on screen under another.
  protected readonly submitError = linkedSignal<
    string,
    readonly string[] | null
  >({ source: this.brewerId, computation: () => null });
  protected readonly confirmed = linkedSignal({
    source: this.brewerId,
    computation: () => false,
  });

  /**
   * The loads start only in the browser. The server renders the loading state, so hydration sees
   * the same markup and the server never calls the API (real data on the server is #1018). The
   * review form waits for it too: a submit before hydration is a native submit that reloads.
   */
  protected readonly browserReady = signal(false);

  constructor() {
    afterNextRender(() => this.browserReady.set(true));

    effect(() => {
      if (!this.browserReady()) return;
      const id = this.brewerId();
      untracked(() => {
        this.detail.load(id);
        if (isGuid(id)) this.beers.load(id);
      });
    });

    // Keyed on the page too, so a page click reloads only the reviews.
    effect(() => {
      if (!this.browserReady()) return;
      const id = this.brewerId();
      const page = this.page();
      if (!isGuid(id)) return;
      untracked(() => this.searchReviews(id, page));
    });
  }

  protected formatDate(iso: string): string {
    return REVIEW_DATE.format(new Date(iso));
  }

  protected retryBrewer(): void {
    this.detail.load(this.brewerId());
  }

  protected retryBeers(): void {
    this.beers.load(this.brewerId());
  }

  protected retryReviews(): void {
    this.searchReviews(this.brewerId(), this.page());
  }

  protected goToPage(page: number): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: page > 1 ? page : null },
      queryParamsHandling: 'merge',
    });
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.submitting()) return;
    this.attempted.set(true);
    if (this.violations().length > 0) return;

    const brewerId = this.brewerId();
    this.submitting.set(true);
    this.submitError.set(null);
    this.confirmed.set(false);
    try {
      await this.reviews.create(brewerId, {
        reviewerName: this.reviewerName(),
        title: this.title(),
        comments: this.comments(),
        rating: this.rating() ?? 0,
      });
      // The user moved to another brewer while this one was in flight. The review was still
      // saved, but this page no longer shows that brewer, so nothing here may react to it.
      if (this.brewerId() !== brewerId) return;

      const failure = this.reviews.error();
      // `create` shares one status with `search`, so a search always follows. It restores the
      // status for the list and shows the saved review on the page it belongs to.
      this.searchReviews(brewerId, this.page());
      if (failure) {
        this.submitError.set(failure.messages);
      } else {
        this.clearForm();
        this.confirmed.set(true);
      }
    } finally {
      this.submitting.set(false);
    }
  }

  private searchReviews(brewerId: string, pageNumber: number): void {
    this.reviews.search(
      brewerId,
      { pageNumber, pageSize: REVIEWS_PAGE_SIZE },
      {},
    );
  }

  private clearForm(): void {
    this.reviewerName.set('');
    this.title.set('');
    this.comments.set('');
    this.rating.set(null);
    this.attempted.set(false);
  }
}
