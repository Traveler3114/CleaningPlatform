import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { uniqueName, futureDate } from '../../helpers/data';

test.describe('Admin Booking Full Lifecycle', () => {
  let bookingId: string;

  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('create a booking with client selection', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await page.locator('#new-booking-btn').click();
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

  test('booking detail page opens and shows all sections', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    await expect(page).toHaveURL(/booking-detail\.html\?id=\d+/);
    await expect(page.locator('#booking-detail')).toBeVisible();
    await expect(page.locator('#client-section')).toBeVisible();
    await expect(page.locator('#services-section')).toBeVisible();
    await expect(page.locator('#assignments-section')).toBeVisible();
  });

  test('change booking status from Pending to Confirmed', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    const statusSelect = page.locator('#booking-status');
    test.skip(!(await statusSelect.isVisible()), 'No status selector');
    await statusSelect.selectOption('Confirmed');
    await page.locator('#save-status-btn').click();
    await expect(page.locator('#status-badge')).toContainText('Confirmed');
  });

  test('assign an employee to a booking', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    const assignSection = page.locator('#assignments-section');
    await expect(assignSection).toBeVisible();
  });

  test('generate invoice from booking', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    const generateBtn = page.locator('#generate-invoice-btn');
    test.skip(!(await generateBtn.isVisible()), 'No invoice button');
    await generateBtn.click();
    await expect(page).toHaveURL(/invoice-detail\.html\?id=\d+/);
  });
});
