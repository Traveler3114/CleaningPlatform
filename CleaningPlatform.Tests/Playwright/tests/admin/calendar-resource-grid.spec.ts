import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Calendar Resource Grid', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('calendar page loads and shows week view', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await expect(page.locator('#resource-grid')).toBeVisible();
    await expect(page.locator('#cal-range-label')).toContainText(/20|Mon|Tue|Wed|Thu|Fri|Sat|Sun/);
  });

  test('switches to day view', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await page.locator('#view-day').click();
    await expect(page.locator('#cal-range-label')).toContainText(/20|Day/);
  });

  test('switches to month view', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await page.locator('#view-month').click();
    await expect(page.locator('#cal-range-label')).toContainText(/20|Month/);
  });

  test('date navigation changes view label', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await expect(page.locator('#cal-range-label')).not.toHaveText('Loading...');
    const initialLabel = await page.locator('#cal-range-label').textContent() || '';
    await page.locator('#nav-next').click();
    await expect(page.locator('#cal-range-label')).not.toHaveText(initialLabel);
    await page.locator('#nav-prev').click();
    await expect(page.locator('#cal-range-label')).toHaveText(initialLabel);
  });

  test('employee filter is visible', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    const filterInput = page.locator('#emp-filter');
    await expect(filterInput).toBeVisible();
  });
});
