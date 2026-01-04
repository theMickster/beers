import { expect, test } from './support/test';

const ICON_NAMES = [
  'Beer',
  'Brewery',
  'Flight',
  'Review',
  'Search',
  'Seasonal',
];

test('theme harness shows all six icons in each theme panel', async ({
  themeHarness,
}) => {
  await themeHarness.goto();

  for (const panel of [themeHarness.paleTheme, themeHarness.stoutTheme]) {
    await expect(panel).toBeVisible();
    for (const name of ICON_NAMES) {
      await expect(panel.getByText(name, { exact: true })).toBeVisible();
    }
    await expect(panel.locator('i.fa-solid')).toHaveCount(ICON_NAMES.length);
  }
});
