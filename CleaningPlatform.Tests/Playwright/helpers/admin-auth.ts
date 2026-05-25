import { Page } from '@playwright/test';
import { BASE_URL, ADMIN_USERNAME, ADMIN_PASSWORD } from './env';

export async function loginAsAdmin(page: Page): Promise<void> {
  await page.goto('/admin/login.html');
  await page.waitForSelector('#login-form');

  await page.fill('#username', ADMIN_USERNAME);
  await page.fill('#password', ADMIN_PASSWORD);
  await page.click('button[type="submit"]');

  await page.waitForURL('**/admin/index.html');
  await page.waitForSelector('.user-name:not(:has-text("Loading..."))');
}

export async function logoutAdmin(page: Page): Promise<void> {
  await page.click('#logout-btn');
  await page.waitForURL('**/admin/login.html');
}
