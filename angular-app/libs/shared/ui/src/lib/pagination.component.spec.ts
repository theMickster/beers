import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';

describe('PaginationComponent', () => {
  let fixture: ComponentFixture<PaginationComponent>;
  let el: HTMLElement;
  let emitted: number[];

  function render(page: number, totalPages: number): void {
    fixture = TestBed.createComponent(PaginationComponent);
    emitted = [];
    fixture.componentInstance.pageChange.subscribe((p) => emitted.push(p));
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('totalPages', totalPages);
    fixture.detectChanges();
    el = fixture.nativeElement as HTMLElement;
  }

  const buttons = () => [...el.querySelectorAll('button')];
  const button = (name: string) =>
    buttons().find(
      (b) =>
        b.textContent?.trim() === name || b.getAttribute('aria-label') === name,
    );
  /** Page numbers and gaps in the order they appear, e.g. `1 … 4 5 6 … 12`. */
  const windowText = () =>
    [...el.querySelectorAll('li')]
      .map((li) => li.textContent?.trim())
      .filter((text) => text !== 'Previous' && text !== 'Next')
      .join(' ');

  it.each([0, 1])('renders nothing when there are %i pages', (totalPages) => {
    render(1, totalPages);

    expect(el.querySelector('nav')).toBeNull();
  });

  describe('windowing', () => {
    it.each([
      [1, 3, '1 2 3'],
      [1, 12, '1 2 … 12'],
      [6, 12, '1 … 5 6 7 … 12'],
      [3, 12, '1 2 3 4 … 12'],
      [12, 12, '1 … 11 12'],
    ])('on page %i of %i shows %s', (page, total, expected) => {
      render(page, total);

      expect(windowText()).toBe(expected);
    });

    it('still shows the last page when the current page is past it', () => {
      render(99, 3);

      expect(windowText()).toBe('1 … 3');
      expect(button('Next')?.disabled).toBe(true);
    });
  });

  it('marks only the current page', () => {
    render(2, 3);

    const current = el.querySelectorAll('[aria-current="page"]');
    expect(current).toHaveLength(1);
    expect(current[0].getAttribute('aria-label')).toBe('Page 2');
    expect(el.querySelector('nav')?.getAttribute('aria-label')).toBe(
      'Pagination',
    );
  });

  describe('Previous and Next', () => {
    it('are disabled at the ends', () => {
      render(1, 3);
      expect(button('Previous')?.disabled).toBe(true);
      expect(button('Next')?.disabled).toBe(false);

      render(3, 3);
      expect(button('Previous')?.disabled).toBe(false);
      expect(button('Next')?.disabled).toBe(true);
    });

    it('move one page', () => {
      render(2, 3);

      button('Previous')?.click();
      button('Next')?.click();

      expect(emitted).toEqual([1, 3]);
    });

    it('send Previous to the last real page when the current page is past the end', () => {
      render(99, 3);

      button('Previous')?.click();

      expect(emitted).toEqual([3]);
    });
  });

  it('emits the page whose button was clicked', () => {
    render(1, 3);

    button('Page 3')?.click();

    expect(emitted).toEqual([3]);
  });

  it('updates the window when the page input changes', () => {
    render(1, 12);

    fixture.componentRef.setInput('page', 6);
    fixture.detectChanges();

    expect(windowText()).toBe('1 … 5 6 7 … 12');
  });
});
