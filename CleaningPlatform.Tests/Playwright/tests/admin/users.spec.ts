import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Users', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('users page loads and shows user list', async ({ page }) => {
    await page.goto('/admin/users.html');
    await expect(page.locator('.breadcrumb strong')).toContainText('Users');
    await expect(page.locator('#users-list')).toBeVisible();
  });

  test('new user button opens modal', async ({ page }) => {
    await page.goto('/admin/users.html');
    await page.locator('#new-user-btn').click();
    await expect(page.locator('#new-user-modal')).toBeVisible();
    await expect(page.locator('#create-user-form')).toBeVisible();
  });

  test('create user form has all required fields', async ({ page }) => {
    await page.goto('/admin/users.html');
    await page.locator('#new-user-btn').click();
    await expect(page.locator('#user-firstname')).toBeVisible();
    await expect(page.locator('#user-lastname')).toBeVisible();
    await expect(page.locator('#user-password')).toBeVisible();
    await expect(page.locator('#user-role')).toBeVisible();
  });
});
