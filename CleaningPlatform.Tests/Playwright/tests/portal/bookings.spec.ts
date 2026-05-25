import { test, expect } from '@playwright/test';
import { loginAsPortalClient } from '../../helpers/portal-auth';

test.describe('Portal Bookings', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsPortalClient(page);
  });

  test('bookings page loads with filter buttons', async ({ page }) => {
    await page.goto('/portal/bookings.html');
    await expect(page.locator('h1')).toContainText('My Bookings');
    await expect(page.locator('#filter-all')).toBeVisible();
    await expect(page.locator('#filter-upcoming')).toBeVisible();
    await expect(page.locator('#filter-completed')).toBeVisible();
    await expect(page.locator('#bookings-list')).toBeVisible();
  });

  test('filter buttons are clickable', async ({ page }) => {
    await page.goto('/portal/bookings.html');
    await page.locator('#filter-upcoming').click();
    await page.locator('#filter-completed').click();
    await page.locator('#filter-all').click();
  });

  test('booking cards link to detail page', async ({ page }) => {
    await page.goto('/portal/bookings.html');
    const bookingCard = page.locator('.booking-card').first();
    if (await bookingCard.isVisible()) {
      await bookingCard.click();
      await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    }
  });

  test('booking detail page shows booking info', async ({ page }) => {
    await page.goto('/portal/bookings.html');
    const bookingCard = page.locator('.booking-card').first();
    if (await bookingCard.isVisible()) {
      await bookingCard.click();
      await expect(page.locator('#booking-detail')).toBeVisible();
    }
  });
});
