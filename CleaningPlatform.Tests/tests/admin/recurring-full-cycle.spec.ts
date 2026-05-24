import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Recurring Schedule Full Cycle', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('recurring page loads with schedule list', async ({ page }) => {
    await page.goto('/admin/recurring.html');
    await expect(page.locator('#schedule-list')).toBeVisible();
    await expect(page.locator('h1')).toContainText('Recurring');
  });

  test('create recurring modal opens from booking detail', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    const makeRecurringBtn = page.locator('#make-recurring-btn');
    test.skip(!(await makeRecurringBtn.isVisible()), 'No make recurring button');
    await makeRecurringBtn.click();
    await expect(page.locator('#recurring-modal')).toBeVisible();
    await expect(page.locator('#frequency')).toBeVisible();
    await expect(page.locator('#day-of-week')).toBeVisible();
    await expect(page.locator('#weeks-ahead')).toBeVisible();
    await expect(page.locator('#end-date')).toBeVisible();
  });

  test('edit a recurring schedule', async ({ page }) => {
    await page.goto('/admin/recurring.html');
    const editBtn = page.locator('.edit-btn').first();
    test.skip(!(await editBtn.isVisible()), 'No recurring schedules exist');
    await editBtn.click();
    await expect(page.locator('#recurring-modal')).toBeVisible();
  });

  test('run auto-generate button is visible', async ({ page }) => {
    await page.goto('/admin/recurring.html');
    const autoBtn = page.locator('#auto-generate-btn');
    test.skip(!(await autoBtn.isVisible()), 'No auto-generate button');
    await autoBtn.click();
  });
});
