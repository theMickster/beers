import type { Locator, Page } from '@playwright/test';

export class ShellPage {
  readonly banner: Locator;
  readonly mainNavigation: Locator;
  readonly heading: Locator;
  readonly footer: Locator;
  readonly themeToggle: Locator;
  readonly menuToggle: Locator;
  readonly mobileNavigation: Locator;

  constructor(private readonly page: Page) {
    this.banner = page.getByRole('banner');
    this.mainNavigation = page.getByRole('navigation', {
      name: 'Main navigation',
    });
    this.heading = page.getByRole('heading', {
      level: 1,
      name: 'Find your next favorite beer.',
    });
    this.footer = page.getByRole('contentinfo');
    this.themeToggle = page.getByRole('button', {
      name: /^Switch to .* theme$/,
    });
    this.menuToggle = page.locator('label.btn[for="bw-nav-drawer"]');
    this.mobileNavigation = page.getByRole('navigation', {
      name: 'Mobile navigation',
    });
  }

  navLink(scope: Locator, name: string): Locator {
    return scope.getByRole('link', { name, exact: true });
  }

  async goto(): Promise<void> {
    await this.page.goto('/');
  }
}
