import type { Locator, Page } from '@playwright/test';

export class SamplesPage {
  readonly heading: Locator;
  readonly primaryButton: Locator;
  readonly infoBadge: Locator;
  readonly infoAlert: Locator;
  readonly ratingGroup: Locator;
  readonly tablist: Locator;
  readonly openModalButton: Locator;
  readonly themeToggle: Locator;

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', {
      level: 1,
      name: 'DaisyUI component samples',
    });
    this.primaryButton = page.getByRole('button', {
      name: 'Primary',
      exact: true,
    });
    this.infoBadge = page.locator('.badge-info');
    this.infoAlert = page.locator('.alert-info');
    this.ratingGroup = page.getByRole('group', { name: 'Rate this beer' });
    this.tablist = page.getByRole('tablist', { name: 'Beer details' });
    this.openModalButton = page.getByRole('button', { name: 'Open modal' });
    this.themeToggle = page.getByRole('button', {
      name: /^Switch to (stout|pale) theme$/,
    });
  }

  async goto(): Promise<void> {
    await this.page.goto('/samples');
  }

  async primaryButtonBackground(): Promise<string> {
    return this.primaryButton.evaluate(
      (element) => getComputedStyle(element).backgroundColor,
    );
  }
}
