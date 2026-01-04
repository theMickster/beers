import { TestBed } from '@angular/core/testing';
import { ThemeService } from '@beers/shared/util';

describe('ThemeService', () => {
  const clean = () => {
    window.localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  };

  beforeEach(clean);
  afterEach(clean);

  const createService = () => TestBed.inject(ThemeService);

  it('defaults to pale when storage is empty', () => {
    expect(createService().current()).toBe('fourteener-pale');
  });

  it('restores stout from storage', () => {
    window.localStorage.setItem('beers-theme', 'fourteener-stout');
    expect(createService().current()).toBe('fourteener-stout');
  });

  it('ignores an invalid stored value', () => {
    window.localStorage.setItem('beers-theme', 'neon');
    expect(createService().current()).toBe('fourteener-pale');
  });

  it('toggle flips the theme, sets data-theme, and persists it', () => {
    const service = createService();
    service.toggle();
    expect(service.current()).toBe('fourteener-stout');
    expect(document.documentElement.getAttribute('data-theme')).toBe(
      'fourteener-stout',
    );
    expect(window.localStorage.getItem('beers-theme')).toBe('fourteener-stout');
    service.toggle();
    expect(service.current()).toBe('fourteener-pale');
    expect(window.localStorage.getItem('beers-theme')).toBe('fourteener-pale');
  });
});
