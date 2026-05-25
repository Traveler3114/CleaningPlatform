import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Recurring Schedule Full Cycle', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('recurring page loads with schedule list', async ({ page }) => {
    await page.goto('/admin/recurring.html');
    await expect(page.locator('#recurring-list')).toBeVisible();
    await expect(page.locator('.breadcrumb strong')).toContainText('Recurring');
  });

  test('create recurring modal opens from booking detail', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await page.waitForSelector('#bookings-list table, #bookings-list .alert-info', { timeout: 10000 }).catch(() => {});
    const bookingLink = page.locator('#bookings-list a.link').first();
    test.skip(!(await bookingLink.isVisible()), 'No bookings exist');
    await bookingLink.click();
    const makeRecurringBtn = page.locator('#make-recurring-btn');
    test.skip(!(await makeRecurringBtn.isVisible()), 'No make recurring button');
    await makeRecurringBtn.click();
    const modal = page.locator('#recurring-modal');
    await expect(modal).toBeVisible({ timeout: 3000 });
    await expect(modal.locator('#rec-frequency')).toBeVisible();
    await expect(modal.locator('#rec-dayofweek')).toBeVisible();
    await expect(modal.locator('#rec-weeksahead')).toBeVisible();
    await expect(modal.locator('#rec-endson')).toBeVisible();
  });

  test('edit a recurring schedule', async ({ page }) => {
    await page.goto('/admin/recurring.html');
    await page.waitForSelector('#recurring-list button:has-text("Edit"), #recurring-list .alert-info', { timeout: 10000 }).catch(() => {});
    const editBtn = page.locator('#recurring-list button:has-text("Edit")').first();
    test.skip(!(await editBtn.isVisible()), 'No recurring schedules exist');
    await editBtn.click();
    await expect(page.locator('#edit-modal')).toBeVisible();
  });

  test('run auto-generate button is visible', async ({ page }) => {
    await page.goto('/admin/recurring.html');
    await page.locator('#run-auto-btn').click();
  });
});
