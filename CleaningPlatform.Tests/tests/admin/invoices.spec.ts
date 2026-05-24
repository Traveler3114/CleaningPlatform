import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Invoices', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('invoices page loads and shows invoice list', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    await expect(page.locator('h1')).toContainText('Invoices');
    await expect(page.locator('#invoices-list')).toBeVisible();
  });

  test('generate invoice form is visible', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    await expect(page.locator('#generate-booking-id')).toBeVisible();
    await expect(page.locator('#generate-invoice-btn')).toBeVisible();
    await expect(page.locator('#generate-invoice-btn')).toHaveText('Generate');
  });

  test('invoice rows link to detail pages', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    await expect(page).toHaveURL(/invoice-detail\.html\?id=\d+/);
  });
});
