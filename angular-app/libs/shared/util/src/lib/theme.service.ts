import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { inject, Injectable, PLATFORM_ID, signal } from '@angular/core';

export type BeersTheme = 'fourteener-pale' | 'fourteener-stout';

const STORAGE_KEY = 'beers-theme';
const DEFAULT_THEME: BeersTheme = 'fourteener-pale';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  readonly current = signal<BeersTheme>(this.readInitialTheme());

  constructor() {
    this.apply(this.current());
  }

  toggle(): void {
    const next = this.current() === 'fourteener-pale' ? 'fourteener-stout' : 'fourteener-pale';
    this.current.set(next);
    this.apply(next);
    if (this.isBrowser) {
      this.document.defaultView?.localStorage.setItem(STORAGE_KEY, next);
    }
  }

  private readInitialTheme(): BeersTheme {
    if (!this.isBrowser) return DEFAULT_THEME;
    const saved = this.document.defaultView?.localStorage.getItem(STORAGE_KEY);
    return saved === 'fourteener-stout' || saved === 'fourteener-pale' ? saved : DEFAULT_THEME;
  }

  private apply(theme: BeersTheme): void {
    this.document.documentElement.setAttribute('data-theme', theme);
  }
}
