import type { Locator, Page } from '@playwright/test';

export class ShellPage {
  readonly banner: Locator;
  readonly mainNavigation: Locator;
  readonly heading: Locator;
  readonly footer: Locator;

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
  }

  async goto(): Promise<void> {
    await this.page.goto('/');
  }
}
