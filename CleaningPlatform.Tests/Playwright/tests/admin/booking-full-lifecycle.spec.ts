import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { futureDate } from '../../helpers/data';

test.describe('Admin Booking Full Lifecycle', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('create a booking with client selection', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await page.locator('#new-booking-btn').click();
    await page.waitForSelector('#client-id option:not([value=""])', { timeout: 10000 }).catch(() => {});
    const clientSelect = page.locator('#client-id');
    const options = clientSelect.locator('option');
    const optionCount = await options.count();
    test.skip(optionCount <= 1, 'No clients available to create booking');
    await clientSelect.selectOption({ index: 1 });
    await page.fill('#booking-date', futureDate(2));
    await page.fill('#booking-hour', '09');
    await page.locator('#create-booking-form button[type="submit"]').click();
    await expect(page.locator('#error-container')).not.toContainText('Network error');
  });

  async function waitForBookingList(page: any) {
    await page.waitForSelector('#bookings-list table, #bookings-list .alert-info', { timeout: 10000 }).catch(() => {});
  }

  test('booking detail page opens and shows all sections', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await waitForBookingList(page);
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    await expect(page.locator('#booking-detail')).toBeVisible();
    await expect(page.locator('h2:has-text("Client Info")')).toBeVisible();
    await expect(page.locator('h2:has-text("Services")')).toBeVisible();
    await expect(page.locator('h2:has-text("Employee Assignments")')).toBeVisible();
  });

  test('change booking status from Pending to Confirmed', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await waitForBookingList(page);
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    await page.waitForSelector('#status-select', { timeout: 5000 });
    const statusSelect = page.locator('#status-select');
    await statusSelect.selectOption('Confirmed');
    await page.locator('#update-status-btn').click();
    await expect(page.locator('#success-container')).toBeVisible({ timeout: 5000 });
  });

  test('assignments section has employee selector', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await waitForBookingList(page);
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    await page.waitForSelector('#employee-select', { timeout: 5000 });
    await expect(page.locator('#employee-select')).toBeVisible();
    await expect(page.locator('#add-assignment-btn')).toBeVisible();
  });

  test('generate invoice from booking', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await waitForBookingList(page);
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    const generateBtn = page.locator('#generate-invoice-btn');
    test.skip(!(await generateBtn.isVisible({ timeout: 5000 }).catch(() => false)), 'No invoice button (booking must be Completed)');
    await generateBtn.click();
    await expect(page).toHaveURL(/invoice-detail\.html\?id=\d+/);
  });
});
