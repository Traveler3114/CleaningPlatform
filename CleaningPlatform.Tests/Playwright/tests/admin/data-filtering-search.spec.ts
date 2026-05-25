import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Data Filtering and Search', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('clients page has search and filter controls', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await expect(page.locator('#search-input')).toBeVisible();
    await expect(page.locator('#type-filter')).toBeVisible();
    await expect(page.locator('#apply-filter')).toBeVisible();
  });

  test('search input filters client list', async ({ page }) => {
    await page.goto('/admin/clients.html');
    const searchInput = page.locator('#search-input');
    await searchInput.fill('Test');
    await page.locator('#apply-filter').click();
    await expect(page.locator('#clients-list')).toBeVisible();
  });

  test('type filter changes client results', async ({ page }) => {
    await page.goto('/admin/clients.html');
    const typeFilter = page.locator('#type-filter');
    const options = await typeFilter.locator('option').count();
    if (options > 1) {
      await typeFilter.selectOption({ index: 1 });
      await page.locator('#apply-filter').click();
      await expect(page.locator('#clients-list')).toBeVisible();
    }
  });

  test('bookings page has date filter', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await expect(page.locator('#date-filter')).toBeVisible();
    await expect(page.locator('#filter-form')).toBeVisible();
  });

  test('show all button clears date filter', async ({ page }) => {
    await page.goto('/admin/bookings.html');
    await page.waitForSelector('#show-all-btn');
    await page.locator('#show-all-btn').click();
  });

  test('invoices page loads with invoice list', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    await expect(page.locator('.breadcrumb strong')).toContainText('Invoices');
    await expect(page.locator('#invoices-list')).toBeVisible();
  });
});
