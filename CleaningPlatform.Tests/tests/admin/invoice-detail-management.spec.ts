import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Invoice Detail Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  async function waitForInvoiceList(page: any) {
    await page.waitForSelector('#invoices-list table, #invoices-list .alert-info', { timeout: 10000 }).catch(() => {});
  }

  test('invoice detail page loads with correct structure', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    await waitForInvoiceList(page);
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    await expect(page).toHaveURL(/invoice-detail\.html\?id=\d+/);
    await expect(page.locator('#invoice-detail')).toBeVisible();
    await expect(page.locator('h2:has-text("Invoice Lines")')).toBeVisible();
    await expect(page.locator('h2:has-text("Payments")')).toBeVisible();
    await expect(page.locator('.stats-grid .kpi-card:has-text("Balance")')).toBeVisible();
  });

  test('update invoice status', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    await waitForInvoiceList(page);
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    await page.waitForSelector('#status-select', { timeout: 5000 });
    const statusSelect = page.locator('#status-select');
    test.skip(!(await statusSelect.isVisible()), 'No status selector');
    await statusSelect.selectOption('Sent');
    await page.locator('#update-status-btn').click();
  });

  test('record a payment on invoice', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    await waitForInvoiceList(page);
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    await page.waitForSelector('#payment-amount', { timeout: 5000 });
    const paymentAmount = page.locator('#payment-amount');
    test.skip(!(await paymentAmount.isVisible()), 'No payment form');
    await paymentAmount.fill('50');
    await page.locator('#record-payment-btn').click();
  });
});
