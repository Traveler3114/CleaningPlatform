import { test, expect } from '@playwright/test';
import { loginAsAdmin, logoutAdmin } from '../../helpers/admin-auth';

test.describe('Admin Auth', () => {
  test('login with valid credentials redirects to dashboard', async ({ page }) => {
    await loginAsAdmin(page);
    await expect(page.locator('h1')).toContainText('Daily View');
  });

  test('login shows username in user pill', async ({ page }) => {
    await loginAsAdmin(page);
    const userName = page.locator('.user-pill-name');
    await expect(userName).not.toContainText('Loading...');
  });

  test('logout redirects to login page', async ({ page }) => {
    await loginAsAdmin(page);
    await logoutAdmin(page);
    await expect(page).toHaveURL(/\/admin\/login\.html/);
  });

  test('invalid credentials show error message', async ({ page }) => {
    await page.goto('/admin/login.html');
    await page.fill('#username', 'wronguser');
    await page.fill('#password', 'wrongpass');
    await page.click('button[type="submit"]');
    await expect(page.locator('#login-error')).toBeVisible();
    await expect(page.locator('#login-error')).not.toHaveText('');
  });

  test('redirects to login when not authenticated', async ({ page }) => {
    await page.goto('/admin/index.html');
    await expect(page).toHaveURL(/\/admin\/login\.html/);
  });
});
