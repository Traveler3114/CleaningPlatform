import { test, expect } from '@playwright/test';
import { loginAsPortalClient, logoutPortalClient } from '../../helpers/portal-auth';

test.describe('Portal Magic Link Auth Flow', () => {
  test('login page has email input and send button', async ({ page }) => {
    await page.goto('/portal/login.html');
    await expect(page.locator('#email')).toBeVisible();
    await expect(page.locator('#send-link-btn')).toBeVisible();
  });

  test('missing token on magic-link page shows error', async ({ page }) => {
    await page.goto('/portal/magic-link.html');
    await expect(page.locator('#status-text')).toBeVisible({ timeout: 5000 });
  });

  test('invalid token on magic-link page shows error', async ({ page }) => {
    await page.goto('/portal/magic-link.html?token=invalidtoken123');
    await expect(page.locator('#status-text')).toBeVisible({ timeout: 5000 });
  });

  test('magic link login redirects to dashboard', async ({ page }) => {
    await loginAsPortalClient(page);
    await expect(page.locator('.user-pill-name')).toBeVisible();
    await expect(page).toHaveURL(/portal\/index\.html/);
  });

  test('navigation tabs are visible after login', async ({ page }) => {
    await loginAsPortalClient(page);
    const tabLinks = page.locator('.nav-tab');
    const tabCount = await tabLinks.count();
    expect(tabCount).toBeGreaterThanOrEqual(3);
  });

  test('logout clears session and redirects to login', async ({ page }) => {
    await loginAsPortalClient(page);
    await logoutPortalClient(page);
    await expect(page).toHaveURL(/portal\/login\.html/);
  });

  test('redirects to login when not authenticated', async ({ page }) => {
    await page.goto('/portal/index.html');
    await expect(page).toHaveURL(/portal\/login\.html/);
  });
});
