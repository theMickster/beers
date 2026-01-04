import { test as base } from '@playwright/test';
import { ShellPage } from '../page-objects/shell.page';
import { ThemeHarnessPage } from '../page-objects/theme-harness.page';

export const test = base.extend<{
  shell: ShellPage;
  themeHarness: ThemeHarnessPage;
}>({
  shell: async ({ page }, use) => {
    await use(new ShellPage(page));
  },
  themeHarness: async ({ page }, use) => {
    await use(new ThemeHarnessPage(page));
  },
});

export { expect } from '@playwright/test';
