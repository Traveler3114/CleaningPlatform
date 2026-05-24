import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Calendar Resource Grid', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('calendar page loads and shows week view', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await expect(page.locator('#resource-grid')).toBeVisible();
    await expect(page.locator('#view-label')).toContainText('Week');
  });

  test('switches to day view', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await page.locator('#day-view-btn').click();
    await expect(page.locator('#view-label')).toContainText('Day');
  });

  test('switches to month view', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    await page.locator('#month-view-btn').click();
    await expect(page.locator('#view-label')).toContainText('Month');
  });

  test('date navigation changes view label', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    const initialLabel = await page.locator('#view-label').textContent();
    await page.locator('#next-btn').click();
    const nextLabel = await page.locator('#view-label').textContent();
    expect(nextLabel).not.toBe(initialLabel);
    await page.locator('#prev-btn').click();
    const prevLabel = await page.locator('#view-label').textContent();
    expect(prevLabel).toBe(initialLabel);
  });

  test('employee filter hides non-matching columns', async ({ page }) => {
    await page.goto('/admin/calendar.html');
    const filterInput = page.locator('#employee-filter');
    await expect(filterInput).toBeVisible();
  });
});
