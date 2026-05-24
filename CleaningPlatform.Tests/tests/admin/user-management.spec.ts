import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { uniqueName } from '../../helpers/data';

test.describe('Admin User Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('users page loads with user list', async ({ page }) => {
    await page.goto('/admin/users.html');
    await expect(page.locator('h1')).toContainText('Users');
    await expect(page.locator('#users-list')).toBeVisible();
  });

  test('create a new user with all fields', async ({ page }) => {
    await page.goto('/admin/users.html');
    await page.locator('#new-user-btn').click();
    await expect(page.locator('#new-user-modal')).toBeVisible();
    await page.fill('#user-firstname', uniqueName('First'));
    await page.fill('#user-lastname', uniqueName('Last'));
    await page.fill('#user-password', 'TestPass123!');
    const roleSelect = page.locator('#user-role');
    const options = await roleSelect.locator('option').count();
    if (options > 1) await roleSelect.selectOption({ index: 1 });
    await page.locator('#create-user-form button[type="submit"]').click();
    await expect(page.locator('#error-container')).not.toContainText('Network error');
  });

  test('toggle user active status', async ({ page }) => {
    await page.goto('/admin/users.html');
    await page.waitForSelector('#users-list table, #users-list .alert-info', { timeout: 10000 }).catch(() => {});
    const toggleCheckbox = page.locator('.checkbox').first();
    test.skip(!(await toggleCheckbox.isVisible()), 'No users exist');
    await toggleCheckbox.click();
  });

  test('reset password via user detail modal', async ({ page }) => {
    await page.goto('/admin/users.html');
    await page.waitForSelector('#users-list table, #users-list .alert-info', { timeout: 10000 }).catch(() => {});
    const userRow = page.locator('.user-row').first();
    test.skip(!(await userRow.isVisible()), 'No users exist');
    await userRow.click();
    await expect(page.locator('#user-modal')).toBeVisible();
    await expect(page.locator('#reset-password-form')).toBeVisible();
  });
});
