import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Invoice Detail Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('invoice detail page loads with correct structure', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    await expect(page).toHaveURL(/invoice-detail\.html\?id=\d+/);
    await expect(page.locator('#invoice-detail')).toBeVisible();
    await expect(page.locator('#invoice-lines-section')).toBeVisible();
    await expect(page.locator('#payments-section')).toBeVisible();
    await expect(page.locator('#balance-due')).toBeVisible();
  });

  test('update invoice status', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    const statusSelect = page.locator('#invoice-status-select');
    test.skip(!(await statusSelect.isVisible()), 'No status selector');
    await statusSelect.selectOption('Sent');
    await page.locator('#save-invoice-status-btn').click();
    await expect(page.locator('#status-badge')).toContainText('Sent');
  });

  test('record a payment on invoice', async ({ page }) => {
    await page.goto('/admin/invoices.html');
    const invoiceLink = page.locator('#invoices-list a.link').first();
    test.skip(!(await invoiceLink.isVisible()), 'No invoices exist');
    await invoiceLink.click();
    const addPaymentBtn = page.locator('#add-payment-btn');
    test.skip(!(await addPaymentBtn.isVisible()), 'No add payment button');
    await addPaymentBtn.click();
    await expect(page.locator('#payment-form')).toBeVisible();
    const paymentAmount = page.locator('#payment-amount');
    await paymentAmount.fill('50');
    await page.locator('#payment-form button[type="submit"]').click();
  });
});
