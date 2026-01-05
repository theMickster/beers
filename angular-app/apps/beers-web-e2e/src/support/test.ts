import { test as base } from '@playwright/test';
import { ShellPage } from '../page-objects/shell.page';
import { SamplesPage } from '../page-objects/samples.page';
import { ThemeHarnessPage } from '../page-objects/theme-harness.page';

export const test = base.extend<{
  samples: SamplesPage;
  shell: ShellPage;
  themeHarness: ThemeHarnessPage;
}>({
  samples: async ({ page }, use) => {
    await use(new SamplesPage(page));
  },
  shell: async ({ page }, use) => {
    await use(new ShellPage(page));
  },
  themeHarness: async ({ page }, use) => {
    await use(new ThemeHarnessPage(page));
  },
});

export { expect } from '@playwright/test';
