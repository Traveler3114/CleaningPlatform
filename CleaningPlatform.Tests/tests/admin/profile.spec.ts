import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Profile', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('profile page loads', async ({ page }) => {
    await page.goto('/admin/profile.html');
    await expect(page.locator('#profile-content')).toBeVisible();
  });

  test('change password form is present', async ({ page }) => {
    await page.goto('/admin/profile.html');
    await expect(page.locator('#change-password-form')).toBeVisible();
    await expect(page.locator('#current-password')).toBeVisible();
    await expect(page.locator('#new-password')).toBeVisible();
  });
});
