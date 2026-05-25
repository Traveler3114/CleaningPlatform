import { test, expect } from '@playwright/test';
import { loginAsPortalClient } from '../../helpers/portal-auth';

test.describe('Portal Invoices', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsPortalClient(page);
  });

  test('invoices page loads with invoice list', async ({ page }) => {
    await page.goto('/portal/invoices.html');
    await expect(page.locator('h1')).toContainText('Invoices');
    await expect(page.locator('#invoices-list')).toBeVisible();
  });

  test('invoice rows link to detail page', async ({ page }) => {
    await page.goto('/portal/invoices.html');
    const invoiceRow = page.locator('.portal-table tbody tr').first();
    if (await invoiceRow.isVisible()) {
      await invoiceRow.click();
      await expect(page).toHaveURL(/invoice-detail\.html\?id=\d+/);
    }
  });

  test('invoice detail page shows invoice info', async ({ page }) => {
    await page.goto('/portal/invoices.html');
    const invoiceRow = page.locator('.portal-table tbody tr').first();
    if (await invoiceRow.isVisible()) {
      await invoiceRow.click();
      await expect(page.locator('#invoice-detail')).toBeVisible();
    }
  });
});
