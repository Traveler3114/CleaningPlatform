import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { uniqueName, uniqueEmail, uniquePhone } from '../../helpers/data';

test.describe('Admin Client Full Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('clients page loads with KPI cards', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await expect(page.locator('h1')).toContainText('Clients');
    await expect(page.locator('#kpi-grid')).toBeVisible();
  });

  test('create a new Person client with all fields', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.locator('#new-client-btn').click();
    await expect(page).toHaveURL(/client-detail\.html/);
    await page.fill('#client-name', uniqueName('Test Person'));
    await page.fill('#primary-email', uniqueEmail('person'));
    await page.fill('#primary-phone', uniquePhone());
    await page.fill('#primary-name', 'Test Person Contact');
    await page.locator('#create-client-form button[type="submit"]').click();
    await expect(page).toHaveURL(/client-detail\.html\?id=\d+/);
  });

  test('create a new Business client', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.locator('#new-client-btn').click();
    await page.fill('#client-name', uniqueName('Test Business'));
    await page.locator('#client-type').selectOption('Business');
    await page.fill('#primary-name', 'Test Biz Contact');
    await page.fill('#primary-email', uniqueEmail('biz'));
    await page.fill('#primary-phone', uniquePhone());
    await page.locator('#create-client-form button[type="submit"]').click();
    await expect(page).toHaveURL(/client-detail\.html\?id=\d+/);
  });

  test('add a site to a client', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.waitForSelector('#clients-list .client-row, #clients-list .alert-info', { timeout: 10000 }).catch(() => {});
    const clientRow = page.locator('.client-row').first();
    test.skip(!(await clientRow.isVisible()), 'No clients exist');
    await clientRow.click();
    await expect(page).toHaveURL(/client-detail\.html\?id=\d+/);
    await page.waitForSelector('#add-site-form');
    await page.fill('#site-name', uniqueName('Site'));
    await page.fill('#site-address', '123 Test St');
    await page.fill('#site-city', 'Test City');
    await page.locator('#add-site-form button[type="submit"]').click();
  });

  test('booking history section is present on client detail', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.waitForSelector('#clients-list .client-row, #clients-list .alert-info', { timeout: 10000 }).catch(() => {});
    const clientRow = page.locator('.client-row').first();
    test.skip(!(await clientRow.isVisible()), 'No clients exist');
    await clientRow.click();
    await expect(page).toHaveURL(/client-detail\.html\?id=\d+/);
    await expect(page.locator('#bookings-display')).toBeVisible();
  });
});
