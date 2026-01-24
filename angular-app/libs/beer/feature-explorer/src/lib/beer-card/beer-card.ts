import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import type { Beer } from '@beers/beer/data-access';

@Component({
  selector: 'bw-beer-card',
  imports: [CurrencyPipe, DecimalPipe, RouterLink],
  templateUrl: './beer-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BeerCardComponent {
  readonly beer = input.required<Beer>();

  /** `rating` is nullable and a beer with no reviews carries an average of 0. */
  protected readonly rating = computed(() => {
    const rating = this.beer().rating;
    return rating && rating.reviewCount > 0 ? rating : null;
  });

  protected readonly priceRange = computed(() => {
    const prices = (this.beer().pricing ?? []).map((tier) => tier.price);
    if (prices.length === 0) return null;
    return { min: Math.min(...prices), max: Math.max(...prices) };
  });
}
