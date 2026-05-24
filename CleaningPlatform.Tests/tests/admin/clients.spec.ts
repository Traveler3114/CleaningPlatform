import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Clients', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('clients page loads and shows client list', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await expect(page.locator('h1')).toContainText('Clients');
    await expect(page.locator('#clients-list')).toBeVisible();
  });

  test('search and filter controls are visible', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await expect(page.locator('#search-input')).toBeVisible();
    await expect(page.locator('#type-filter')).toBeVisible();
    await expect(page.locator('#apply-filter')).toBeVisible();
  });

  test('new client button navigates to creation page', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.locator('#new-client-btn').click();
    await expect(page).toHaveURL(/client-detail\.html\?new=1/);
  });

  test('create client form has all required fields', async ({ page }) => {
    await page.goto('/admin/client-detail.html?new=1');
    await expect(page.locator('#client-name')).toBeVisible();
    await expect(page.locator('#client-type')).toBeVisible();
    await expect(page.locator('#primary-name')).toBeVisible();
    await expect(page.locator('#primary-phone')).toBeVisible();
    await expect(page.locator('#primary-email')).toBeVisible();
    await expect(page.locator('#create-client-form button[type="submit"]')).toHaveText('Create Client');
  });

  test('can create a new Person client', async ({ page }) => {
    await page.goto('/admin/client-detail.html?new=1');

    const uniqueId = Date.now();
    await page.fill('#client-name', `Test Client ${uniqueId}`);
    await page.selectOption('#client-type', 'Person');
    await page.fill('#primary-name', `Primary ${uniqueId}`);
    await page.fill('#primary-phone', `+385 91 ${uniqueId}`.slice(0, 20));
    await page.fill('#primary-email', `client${uniqueId}@test.com`);

    await page.locator('#create-client-form button[type="submit"]').click();
    await expect(page.locator('#success-message').first()).toBeVisible({ timeout: 5000 });
  });

  test('KPI cards are visible on clients page', async ({ page }) => {
    await page.goto('/admin/clients.html');
    const kpiGrid = page.locator('#kpi-grid');
    await expect(kpiGrid).toBeVisible();
  });
});
