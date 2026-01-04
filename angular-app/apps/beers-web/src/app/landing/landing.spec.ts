import { TestBed } from '@angular/core/testing';
import { LandingComponent } from './landing';

describe('LandingComponent', () => {
  it('renders the hero heading and image', () => {
    const fixture = TestBed.createComponent(LandingComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Find your next favorite beer',
    );
    expect(
      compiled.querySelector('img[src="/beers-banner-hero.jpeg"]'),
    ).toBeTruthy();
  });
});
