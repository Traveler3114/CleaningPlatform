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
    await expect(page.locator('#kpi-cards')).toBeVisible();
  });

  test('create a new Person client with all fields', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.locator('#new-client-btn').click();
    await expect(page).toHaveURL(/client-detail\.html/);
    await page.fill('#client-name', uniqueName('Test Person'));
    await page.fill('#client-email', uniqueEmail('person'));
    await page.fill('#client-phone', uniquePhone());
    await page.locator('#save-client-btn').click();
    await expect(page.locator('#success-message')).toBeVisible();
  });

  test('create a new Business client', async ({ page }) => {
    await page.goto('/admin/clients.html');
    await page.locator('#new-client-btn').click();
    await page.fill('#client-name', uniqueName('Test Business'));
    await page.locator('#client-type').selectOption('Business');
    await page.fill('#client-email', uniqueEmail('biz'));
    await page.fill('#client-phone', uniquePhone());
    await page.locator('#save-client-btn').click();
  });

  test('add a site to a client', async ({ page }) => {
    await page.goto('/admin/clients.html');
    const clientLink = page.locator('#clients-list a.link').first();
    test.skip(!(await clientLink.isVisible()), 'No clients exist');
    await clientLink.click();
    const addSiteBtn = page.locator('#add-site-btn');
    test.skip(!(await addSiteBtn.isVisible()), 'No add site button');
    await addSiteBtn.click();
    await expect(page.locator('#site-form')).toBeVisible();
    await page.fill('#site-name', uniqueName('Site'));
    await page.fill('#site-address', '123 Test St');
    await page.fill('#site-city', 'Test City');
    await page.locator('#site-form button[type="submit"]').click();
  });

  test('booking history section is present on client detail', async ({ page }) => {
    await page.goto('/admin/clients.html');
    const clientLink = page.locator('#clients-list a.link').first();
    test.skip(!(await clientLink.isVisible()), 'No clients exist');
    await clientLink.click();
    await expect(page.locator('#booking-history-section')).toBeVisible();
  });
});
