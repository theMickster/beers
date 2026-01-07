import { Component } from '@angular/core';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { DOCUMENT } from '@angular/common';
import { Router, provideRouter } from '@angular/router';
import { AppLayoutComponent } from './app-layout';

@Component({ template: '' })
class StubComponent {}

const PATHS = ['/explorer', '/brewers', '/blog', '/dashboard', '/compare'];

describe('AppLayoutComponent', () => {
  let fixture: ComponentFixture<AppLayoutComponent>;
  let el: HTMLElement;

  beforeEach(async () => {
    window.localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [AppLayoutComponent],
      providers: [
        provideRouter(
          PATHS.map((p) => ({ path: p.slice(1), component: StubComponent })),
        ),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppLayoutComponent);
    el = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  const query = <T extends Element>(selector: string): T => {
    const found = el.querySelector<T>(selector);
    if (!found) throw new Error(`Missing element: ${selector}`);
    return found;
  };
  const checkbox = () => query<HTMLInputElement>('.drawer-toggle');
  const setOpen = (open: boolean) => {
    checkbox().checked = open;
    checkbox().dispatchEvent(new Event('change'));
    fixture.detectChanges();
  };

  it('renders the logo', () => {
    expect(
      el.querySelector('img[src="/logos/beers-logo-badge.png"]'),
    ).toBeTruthy();
    expect(
      el.querySelector('a[aria-label="Beers home"]')?.getAttribute('href'),
    ).toBe('/');
  });

  it('renders the five nav links and no samples link', () => {
    const hrefs = Array.from(
      el.querySelectorAll('nav[aria-label="Main navigation"] a'),
    ).map((a) => a.getAttribute('href'));
    expect(hrefs).toEqual(PATHS);
    expect(el.querySelector('a[href="/samples"]')).toBeNull();
  });

  it('renders the router outlet inside main', () => {
    expect(el.querySelector('main router-outlet')).toBeTruthy();
  });

  it('toggles and persists the theme', () => {
    query<HTMLButtonElement>('button[title="Change color theme"]').click();
    expect(
      TestBed.inject(DOCUMENT).documentElement.getAttribute('data-theme'),
    ).toBe('fourteener-stout');
    expect(window.localStorage.getItem('beers-theme')).toBe('fourteener-stout');
  });

  it('opens and closes the drawer from the checkbox', () => {
    const content = query('.drawer-content');
    expect(content.hasAttribute('inert')).toBe(false);

    setOpen(true);
    expect(content.hasAttribute('inert')).toBe(true);

    setOpen(false);
    expect(content.hasAttribute('inert')).toBe(false);
  });

  it('closes the drawer when a mobile link is clicked', () => {
    setOpen(true);
    query<HTMLAnchorElement>('nav[aria-label="Mobile navigation"] a').click();
    fixture.detectChanges();
    expect(checkbox().checked).toBe(false);
  });

  it('closes the drawer on Escape', () => {
    setOpen(true);
    document.dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }),
    );
    fixture.detectChanges();
    expect(checkbox().checked).toBe(false);
  });

  it('marks the active link with aria-current', async () => {
    await TestBed.inject(Router).navigateByUrl('/blog');
    fixture.detectChanges();
    const current = el.querySelectorAll('a[aria-current="page"]');
    expect(current.length).toBeGreaterThan(0);
    current.forEach((a) => expect(a.getAttribute('href')).toBe('/blog'));
  });
});
