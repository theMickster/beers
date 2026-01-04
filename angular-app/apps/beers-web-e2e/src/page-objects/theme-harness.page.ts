import type { Locator, Page } from '@playwright/test';

export class ThemeHarnessPage {
  readonly paleTheme: Locator;
  readonly stoutTheme: Locator;

  constructor(private readonly page: Page) {
    this.paleTheme = page.getByRole('region', { name: 'Pale theme' });
    this.stoutTheme = page.getByRole('region', { name: 'Stout theme' });
  }

  async goto(): Promise<void> {
    await this.page.goto('/dev/theme-harness');
  }
}
