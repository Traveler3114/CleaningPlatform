import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Reports', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('reports page loads', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('.breadcrumb strong')).toContainText('Reports');
  });

  test('dashboard stats section is visible', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('#dashboard-stats')).toBeVisible();
  });

  test('revenue table section is visible', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('#revenue-table')).toBeVisible();
  });

  test('top clients section is visible', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('#top-clients-table')).toBeVisible();
  });

  test('employee utilization section is visible', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('#utilization-table')).toBeAttached();
  });

  test('completion rates section is visible', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('#completion-table')).toBeVisible();
  });

  test('export button is visible', async ({ page }) => {
    await page.goto('/admin/reports.html');
    await expect(page.locator('#export-btn')).toBeVisible();
    await expect(page.locator('#export-btn')).toHaveText('Export invoices to Excel');
  });
});
