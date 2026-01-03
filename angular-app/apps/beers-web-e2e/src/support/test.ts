import { test as base } from '@playwright/test';
import { ShellPage } from '../page-objects/shell.page';

export const test = base.extend<{ shell: ShellPage }>({
  shell: async ({ page }, use) => {
    await use(new ShellPage(page));
  },
});

export { expect } from '@playwright/test';
