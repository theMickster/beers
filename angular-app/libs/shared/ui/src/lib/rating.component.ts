import { DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';

/**
 * Defined here because a `type:ui` lib cannot import the domain libs. It matches `BeerRating`
 * and `BrewerRating` structurally.
 */
export interface RatingSummary {
  readonly average: number;
  readonly reviewCount: number;
}

/** Star, average and review count. A missing rating and a rating with no reviews read the same. */
@Component({
  selector: 'bw-rating',
  imports: [DecimalPipe],
  template: `
    @if (reviewed(); as rating) {
      <i class="fa-solid fa-star text-warning" aria-hidden="true"></i>
      <span class="font-semibold">{{ rating.average | number: '1.1-1' }}</span>
      <span class="opacity-70">
        ({{ rating.reviewCount }}
        {{ rating.reviewCount === 1 ? 'review' : 'reviews' }})
      </span>
    } @else {
      <span class="opacity-60">No reviews yet</span>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RatingComponent {
  readonly rating = input.required<RatingSummary | null>();

  /** A beer or brewer with no reviews carries an average of 0, which is not a real rating. */
  protected readonly reviewed = computed(() => {
    const rating = this.rating();
    return rating && rating.reviewCount > 0 ? rating : null;
  });
}
