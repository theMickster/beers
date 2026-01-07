import { expect, test } from './support/test';

const NAV = [
  { label: 'Explorer', path: '/explorer' },
  { label: 'Brewers', path: '/brewers' },
  { label: 'Blog', path: '/blog' },
  { label: 'Dashboard', path: '/dashboard' },
  { label: 'Compare', path: '/compare' },
];

test.describe('desktop', () => {
  test.use({ viewport: { width: 1280, height: 800 } });

  test('app loads and shows the shell', async ({ page, shell }) => {
    await shell.goto();

    await expect(page).toHaveTitle('Beers — Colorado Craft');
    await expect(shell.banner).toBeVisible();
    await expect(shell.mainNavigation).toBeVisible();
    await expect(shell.heading).toBeVisible();
    await expect(shell.footer).toBeVisible();
    await expect(shell.themeToggle).toBeVisible();
    await expect(shell.menuToggle).toBeHidden();
    for (const { label, path } of NAV) {
      await expect(shell.navLink(shell.mainNavigation, label)).toHaveAttribute(
        'href',
        path,
      );
    }
  });

  for (const { label, path } of NAV) {
    test(`${label} link navigates to ${path}`, async ({ page, shell }) => {
      await shell.goto();
      await shell.navLink(shell.mainNavigation, label).click();

      await expect(page).toHaveURL(path);
      await expect(
        page.getByRole('heading', { level: 1, name: label }),
      ).toBeVisible();
      await expect(shell.navLink(shell.mainNavigation, label)).toHaveAttribute(
        'aria-current',
        'page',
      );
    });
  }
});

test.describe('mobile', () => {
  test.use({ viewport: { width: 375, height: 800 } });

  test('shows a hamburger instead of the desktop nav', async ({
    page,
    shell,
  }) => {
    await shell.goto();

    await expect(shell.menuToggle).toBeVisible();
    await expect(shell.mainNavigation).toBeHidden();
    await expect(shell.mobileNavigation).toBeHidden();
    expect(
      await page.evaluate(
        () =>
          document.documentElement.scrollWidth <=
          document.documentElement.clientWidth,
      ),
    ).toBe(true);
  });

  test('opens the drawer, navigates and closes it', async ({ page, shell }) => {
    await shell.goto();
    await shell.menuToggle.click();

    await expect(shell.mobileNavigation).toBeVisible();
    for (const { label } of NAV) {
      await expect(shell.navLink(shell.mobileNavigation, label)).toBeVisible();
    }

    await shell.navLink(shell.mobileNavigation, 'Brewers').click();

    await expect(page).toHaveURL('/brewers');
    await expect(shell.mobileNavigation).toBeHidden();
    await expect(
      page.getByRole('heading', { level: 1, name: 'Brewers' }),
    ).toBeVisible();
  });
});
