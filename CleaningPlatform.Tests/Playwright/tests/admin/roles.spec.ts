import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Roles', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('roles page loads and shows role list', async ({ page }) => {
    await page.goto('/admin/roles.html');
    await expect(page.locator('h1')).toContainText('Roles');
    await expect(page.locator('#roles-list')).toBeVisible();
  });

  test('new role button opens modal', async ({ page }) => {
    await page.goto('/admin/roles.html');
    await page.locator('#new-role-btn').click();
    await expect(page.locator('#role-modal')).toBeVisible();
    await expect(page.locator('#role-create-form')).toBeVisible();
  });

  test('create role form has name field and permissions grid', async ({ page }) => {
    await page.goto('/admin/roles.html');
    await page.locator('#new-role-btn').click();
    await expect(page.locator('#role-name')).toBeVisible();
    await expect(page.locator('#permissions-grid')).toBeVisible();
  });
});
