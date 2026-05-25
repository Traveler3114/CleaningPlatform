import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Services', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('services page loads and shows service list', async ({ page }) => {
    await page.goto('/admin/services.html');
    await expect(page.locator('h1')).toContainText('Services');
    await expect(page.locator('#services-list')).toBeVisible();
  });

  test('create service button opens modal', async ({ page }) => {
    await page.goto('/admin/services.html');
    await page.locator('#new-service-btn').click();
    await expect(page.locator('#service-modal')).toBeVisible();
    await expect(page.locator('#service-form')).toBeVisible();
  });

  test('service form has all required fields', async ({ page }) => {
    await page.goto('/admin/services.html');
    await page.locator('#new-service-btn').click();
    await expect(page.locator('#service-code')).toBeVisible();
    await expect(page.locator('#service-name')).toBeVisible();
    await expect(page.locator('#service-category')).toBeVisible();
    await expect(page.locator('#service-unit')).toBeVisible();
  });

  test('can create a new service', async ({ page }) => {
    await page.goto('/admin/services.html');
    await page.locator('#new-service-btn').click();

    const uniqueId = Date.now();
    await page.fill('#service-code', `TEST${uniqueId}`);
    await page.fill('#service-name', `Test Service ${uniqueId}`);
    await page.fill('#service-category', 'Test');
    await page.fill('#service-unit', 'visit');
    await page.fill('#service-price-min', '50');
    await page.fill('#service-price-max', '150');
    await page.fill('#service-price-avg', '100');

    await page.click('button:has-text("Save")');
  });
});
