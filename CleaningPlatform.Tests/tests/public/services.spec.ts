import { test, expect } from '@playwright/test';

test.describe('Public Services Page', () => {
  test('services page loads and shows service catalog', async ({ page }) => {
    await page.goto('/public/services.html');
    await expect(page.locator('h1')).toContainText('Services');
    await expect(page.locator('#services-grid')).toBeVisible();
  });

  test('category filter is visible', async ({ page }) => {
    await page.goto('/public/services.html');
    await expect(page.locator('#category-filter')).toBeVisible();
  });

  test('navigation links are present', async ({ page }) => {
    await page.goto('/public/services.html');
    const navLinks = page.locator('.nav-link');
    const linkTexts = await navLinks.allTextContents();
    expect(linkTexts.map(t => t.trim())).toEqual(
      expect.arrayContaining(['Services', 'Book Now', 'Sign In'])
    );
  });
});
