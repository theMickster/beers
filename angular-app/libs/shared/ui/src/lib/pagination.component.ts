import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';

/** Page numbers to show, with `null` standing in for a gap: 1 … 4 5 6 … 12. */
function pageWindow(current: number, total: number): (number | null)[] {
  const pages = new Set(
    [1, current - 1, current, current + 1, total].filter(
      (page) => page >= 1 && page <= total,
    ),
  );
  const sorted = [...pages].sort((a, b) => a - b);
  const result: (number | null)[] = [];
  for (const page of sorted) {
    const previous = result[result.length - 1];
    if (typeof previous === 'number' && page - previous > 1) {
      result.push(null);
    }
    result.push(page);
  }
  return result;
}

/**
 * Previous, windowed page buttons and Next. It owns no state: the parent holds the page (usually
 * in the URL) and reacts to `pageChange`. There is no label input because a page has one pager.
 */
@Component({
  selector: 'bw-pagination',
  template: `
    @if (totalPages() > 1) {
      <nav aria-label="Pagination" class="flex justify-center">
        <ul class="join">
          <li>
            <button
              type="button"
              class="btn join-item"
              [disabled]="page() <= 1"
              (click)="previous()"
            >
              Previous
            </button>
          </li>
          @for (entry of window(); track $index) {
            <li>
              @if (entry === null) {
                <span class="btn join-item btn-disabled" aria-hidden="true"
                  >…</span
                >
              } @else {
                <button
                  type="button"
                  class="btn join-item"
                  [class.btn-active]="entry === page()"
                  [attr.aria-current]="entry === page() ? 'page' : null"
                  [attr.aria-label]="'Page ' + entry"
                  (click)="pageChange.emit(entry)"
                >
                  {{ entry }}
                </button>
              }
            </li>
          }
          <li>
            <button
              type="button"
              class="btn join-item"
              [disabled]="page() >= totalPages()"
              (click)="pageChange.emit(page() + 1)"
            >
              Next
            </button>
          </li>
        </ul>
      </nav>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginationComponent {
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly pageChange = output<number>();

  protected readonly window = computed(() =>
    pageWindow(this.page(), this.totalPages()),
  );

  /**
   * A page past the last one (a hand-edited URL) shows an empty list, so Previous lands on the
   * last real page instead of one more empty page.
   */
  protected previous(): void {
    this.pageChange.emit(Math.min(this.page() - 1, this.totalPages()));
  }
}
