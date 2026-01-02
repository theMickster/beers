import { TestBed, ComponentFixture } from '@angular/core/testing';
import { AppComponent } from './app';
import { DOCUMENT } from '@angular/common';

describe('App', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the app component', () => {
    expect(component).toBeTruthy();
  });

  it('renders the Beers landing shell', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Find your next favorite beer');
    expect(compiled.querySelector('img[src="/logos/beers-logo-badge.png"]')).toBeTruthy();
    expect(compiled.querySelector('img[src="/beers-banner-hero.jpeg"]')).toBeTruthy();
  });

  it('persists the selected theme when toggled', () => {
    fixture.nativeElement.querySelector('button').click();
    expect(TestBed.inject(DOCUMENT).documentElement.getAttribute('data-theme')).toBe('fourteener-stout');
    expect(window.localStorage.getItem('beers-theme')).toBe('fourteener-stout');
  });

  it('should apply change detection strategy OnPush', () => {
    const metadata = (AppComponent as unknown as { ɵcmp: { onPush: boolean } })['ɵcmp'];
    expect(metadata.onPush).toBeTruthy();
  });
});
