import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { uniqueName, uniqueEmail } from '../../helpers/data';

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
    await expect(page.locator('#user-modal')).toBeVisible();
    const username = `testuser${Date.now()}`;
    await page.fill('#username', username);
    await page.fill('#fullname', uniqueName('Test User'));
    await page.fill('#email', uniqueEmail('user'));
    await page.fill('#password', 'TestPass123!');
    const roleSelect = page.locator('#role-id');
    const options = await roleSelect.locator('option').count();
    if (options > 1) await roleSelect.selectOption({ index: 1 });
    await page.locator('#user-form button[type="submit"]').click();
    await expect(page.locator('#success-message').first()).toBeVisible({ timeout: 5000 });
  });

  test('toggle user active status', async ({ page }) => {
    await page.goto('/admin/users.html');
    const toggleCheckbox = page.locator('.active-toggle').first();
    test.skip(!(await toggleCheckbox.isVisible()), 'No users exist');
    await toggleCheckbox.click();
  });

  test('reset password for a user', async ({ page }) => {
    await page.goto('/admin/users.html');
    const userRow = page.locator('#users-list tr').first();
    test.skip(!(await userRow.isVisible()), 'No users exist');
    await userRow.click();
  });
});
