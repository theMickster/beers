import { TestBed } from '@angular/core/testing';
import { RatingComponent, type RatingSummary } from './rating.component';

function render(rating: RatingSummary | null): string {
  const fixture = TestBed.createComponent(RatingComponent);
  fixture.componentRef.setInput('rating', rating);
  fixture.detectChanges();
  return (fixture.nativeElement as HTMLElement).textContent
    .replace(/\s+/g, ' ')
    .trim();
}

describe('RatingComponent', () => {
  it('shows the average to one decimal and a plural review count', () => {
    expect(render({ average: 4.25, reviewCount: 12 })).toBe('4.3 (12 reviews)');
  });

  it('uses the singular for one review', () => {
    expect(render({ average: 5, reviewCount: 1 })).toBe('5.0 (1 review)');
  });

  // A beer with no reviews carries an average of 0, so the count decides, not the object.
  it.each([
    ['has no rating', null],
    ['has a rating with no reviews', { average: 0, reviewCount: 0 }],
  ])('says there are no reviews when it %s', (_, rating) => {
    expect(render(rating)).toBe('No reviews yet');
  });

  it('updates when the rating changes', () => {
    const fixture = TestBed.createComponent(RatingComponent);
    fixture.componentRef.setInput('rating', null);
    fixture.detectChanges();

    fixture.componentRef.setInput('rating', { average: 3, reviewCount: 2 });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('3.0');
  });
});
