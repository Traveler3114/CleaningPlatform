import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin SOPs', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('SOPs page loads and shows template list', async ({ page }) => {
    await page.goto('/admin/sops.html');
    await expect(page.locator('h1')).toContainText('SOPs');
    await expect(page.locator('#sops-list')).toBeVisible();
  });

  test('create template form has required fields', async ({ page }) => {
    await page.goto('/admin/sops.html');
    await expect(page.locator('#template-name')).toBeVisible();
    await expect(page.locator('#template-service-type')).toBeVisible();
    await expect(page.locator('#create-template-form button[type="submit"]')).toHaveText('Create Template');
  });

  test('can create a new SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');

    const uniqueName = `Test SOP ${Date.now()}`;
    await page.fill('#template-name', uniqueName);
    await page.selectOption('#template-service-type', 'SiteBased');

    await page.locator('#create-template-form button[type="submit"]').click();
  });
});
