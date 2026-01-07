import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { ComingSoonComponent } from './coming-soon';

describe('ComingSoonComponent', () => {
  it('renders the route title', () => {
    TestBed.configureTestingModule({
      imports: [ComingSoonComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { data: { title: 'Brewers' } } },
        },
      ],
    });
    const fixture = TestBed.createComponent(ComingSoonComponent);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toContain('Brewers');
    expect(el.textContent).toContain('Coming soon');
  });
});
