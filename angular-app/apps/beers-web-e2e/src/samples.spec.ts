import { expect, test } from './support/test';

test('samples page re-themes in place when the header toggle is flipped', async ({
  page,
  samples,
}) => {
  await samples.goto();

  await expect(samples.heading).toBeVisible();
  await expect(samples.primaryButton).toBeVisible();
  await expect(samples.infoBadge).toBeVisible();
  await expect(samples.infoAlert).toBeVisible();
  await expect(samples.ratingGroup).toBeVisible();
  await expect(samples.tablist).toBeVisible();
  await expect(samples.openModalButton).toBeVisible();

  const html = page.locator('html');
  await expect(html).toHaveAttribute('data-theme', 'fourteener-pale');
  const paleBackground = await samples.primaryButtonBackground();

  // A window property survives only if the page is not reloaded or navigated.
  await page.evaluate(() => {
    (window as unknown as { samplesMarker: boolean }).samplesMarker = true;
  });

  await samples.themeToggle.click();
  await expect(html).toHaveAttribute('data-theme', 'fourteener-stout');
  await expect
    .poll(() => samples.primaryButtonBackground())
    .not.toBe(paleBackground);

  await samples.themeToggle.click();
  await expect(html).toHaveAttribute('data-theme', 'fourteener-pale');
  await expect
    .poll(() => samples.primaryButtonBackground())
    .toBe(paleBackground);

  expect(
    await page.evaluate(
      () => (window as unknown as { samplesMarker?: boolean }).samplesMarker,
    ),
  ).toBe(true);
});
