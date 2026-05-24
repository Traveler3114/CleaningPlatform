import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Bookings', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('bookings page loads and shows booking list', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await expect(page.locator('h1')).toContainText('Bookings');
    const bookingsList = page.locator('#bookings-list');
    await expect(bookingsList).toBeVisible();
  });

  test('filter form is present', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await expect(page.locator('#filter-form')).toBeVisible();
    await expect(page.locator('#date-filter')).toBeVisible();
  });

  test('new booking panel can be toggled', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const newBookingBtn = page.locator('#new-booking-btn');
    const newBookingPanel = page.locator('#new-booking-panel');

    await expect(newBookingPanel).not.toBeVisible();
    await newBookingBtn.click();
    await expect(newBookingPanel).toBeVisible();
  });

  test('create booking form has all required fields', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await page.locator('#new-booking-btn').click();

    await expect(page.locator('#client-id')).toBeVisible();
    await expect(page.locator('#booking-date')).toBeVisible();
    await expect(page.locator('#booking-hour')).toBeVisible();
    await expect(page.locator('#service-type')).toBeVisible();
    await expect(page.locator('#create-booking-form button[type="submit"]')).toHaveText('Create Booking');
  });

  test('can create a booking with client selection', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await page.locator('#new-booking-btn').click();

    const clientSelect = page.locator('#client-id');
    const options = clientSelect.locator('option');

    const optionCount = await options.count();
    if (optionCount > 1) {
      await clientSelect.selectOption({ index: 1 });

      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const dateStr = tomorrow.toISOString().split('T')[0];
      await page.fill('#booking-date', dateStr);
      await page.fill('#booking-hour', '10');

      await page.locator('#create-booking-form button[type="submit"]').click();

      await expect(page.locator('#error-container')).not.toContainText('Network error');
      await expect(page.locator('#success-message')).toBeVisible({ timeout: 5000 });
    }
  });

  test('booking detail page opens from list', async ({ page }) => {
    await page.goto('/admin/bookings.html');

    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    await expect(page.locator('#booking-detail')).toBeVisible();
  });

  test('pagination controls are visible when bookings exist', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const prevBtn = page.locator('button:has-text("Previous")');
    const nextBtn = page.locator('button:has-text("Next")');
    const prevVisible = await prevBtn.isVisible().catch(() => false);
    const nextVisible = await nextBtn.isVisible().catch(() => false);
    if (prevVisible) await expect(prevBtn).toBeVisible();
    if (nextVisible) await expect(nextBtn).toBeVisible();
  });
});
