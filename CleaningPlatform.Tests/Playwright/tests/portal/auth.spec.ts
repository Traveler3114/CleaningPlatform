import { test, expect } from '@playwright/test';
import { loginAsPortalClient, logoutPortalClient } from '../../helpers/portal-auth';

test.describe('Portal Auth (Magic Link)', () => {
  test('login page has email input and send button', async ({ page }) => {
    await page.goto('/portal/login.html');
    await expect(page.locator('#email')).toBeVisible();
    await expect(page.locator('#send-link-btn')).toHaveText('Send Magic Link');
  });

  test('magic link login redirects to dashboard', async ({ page }) => {
    await loginAsPortalClient(page);
    await expect(page.locator('h1')).toContainText('Dashboard');
  });

  test('user pill shows client name after login', async ({ page }) => {
    await loginAsPortalClient(page);
    const userName = page.locator('.user-pill-name');
    await expect(userName).not.toContainText('Loading...');
  });

  test('logout clears session and redirects to login', async ({ page }) => {
    await loginAsPortalClient(page);
    await logoutPortalClient(page);
    await expect(page).toHaveURL(/\/portal\/login\.html/);
  });

  test('redirects to login when not authenticated', async ({ page }) => {
    await page.goto('/portal/index.html');
    await expect(page).toHaveURL(/\/portal\/login\.html/);
  });

  test('navigation tabs are visible on dashboard', async ({ page }) => {
    await loginAsPortalClient(page);
    const navTabs = page.locator('.nav-tab');
    const tabTexts = await navTabs.allTextContents();
    expect(tabTexts.map(t => t.trim())).toEqual(
      expect.arrayContaining(['Dashboard', 'Bookings', 'Invoices', 'Profile'])
    );
  });
});
