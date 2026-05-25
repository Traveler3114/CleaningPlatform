import { test, expect } from '@playwright/test';
import { loginAsPortalClient } from '../../helpers/portal-auth';

test.describe('Portal Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsPortalClient(page);
  });

  test('KPI grid shows dashboard stats', async ({ page }) => {
    const kpiGrid = page.locator('#kpi-grid');
    await expect(kpiGrid).toBeVisible();
    const kpiCards = page.locator('.kpi-card');
    const count = await kpiCards.count();
    expect(count).toBeGreaterThanOrEqual(1);
  });

  test('upcoming bookings section is present', async ({ page }) => {
    await expect(page.locator('#upcoming-bookings')).toBeVisible();
  });

  test('recent invoices section is present', async ({ page }) => {
    await expect(page.locator('#recent-invoices')).toBeVisible();
  });
});
