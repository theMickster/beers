import { expect, test } from './support/test';

test('app loads and shows the shell', async ({ page, shell }) => {
  await shell.goto();

  await expect(page).toHaveTitle('Beers — Colorado Craft');
  await expect(shell.banner).toBeVisible();
  await expect(shell.mainNavigation).toBeVisible();
  await expect(shell.heading).toBeVisible();
  await expect(shell.footer).toBeVisible();
});
