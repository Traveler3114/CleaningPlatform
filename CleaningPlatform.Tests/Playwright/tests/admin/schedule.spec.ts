import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Schedule', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('schedule page loads', async ({ page }) => {
    await page.goto('/admin/schedule.html');
    await expect(page.locator('h1')).toContainText('Schedule');
    await expect(page.locator('#schedule-list')).toBeVisible();
  });

  test('add day button opens modal', async ({ page }) => {
    await page.goto('/admin/schedule.html');
    await page.locator('#add-day-btn').click();
    await expect(page.locator('#schedule-modal')).toBeVisible();
    await expect(page.locator('#schedule-add-form')).toBeVisible();
  });

  test('schedule form has all required fields', async ({ page }) => {
    await page.goto('/admin/schedule.html');
    await page.locator('#add-day-btn').click();
    await expect(page.locator('#schedule-day')).toBeVisible();
    await expect(page.locator('#schedule-start')).toBeVisible();
    await expect(page.locator('#schedule-end')).toBeVisible();
    await expect(page.locator('#schedule-capacity')).toBeVisible();
  });

  test('override section is accessible', async ({ page }) => {
    await page.goto('/admin/schedule.html');
    await expect(page.locator('#open-override-modal')).toBeVisible();
    await expect(page.locator('#overrides-list')).toBeVisible();
  });
});
