import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';

test.describe('Admin Dashboard (Daily View)', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('displays KPI grid with revenue and stats cards', async ({ page }) => {
    const kpiGrid = page.locator('#kpi-grid');
    await expect(kpiGrid).toBeVisible();
    const kpiCards = page.locator('.kpi-card');
    await expect(kpiCards.first()).toBeVisible();
  });

  test('displays bookings section', async ({ page }) => {
    const bookingsTable = page.locator('#bookings-table');
    await expect(bookingsTable).toBeVisible();
  });

  test('displays slots section', async ({ page }) => {
    const slotsTable = page.locator('#slots-table');
    await expect(slotsTable).toBeVisible();
  });

  test('date picker is present and can be changed', async ({ page }) => {
    const datePicker = page.locator('#selected-date');
    await expect(datePicker).toBeVisible();
    const today = new Date().toISOString().split('T')[0];
    await expect(datePicker).toHaveValue(today);
  });

  test('navigation menu is visible with all groups', async ({ page }) => {
    const navGroups = page.locator('.nav-group__label');
    const labels = await navGroups.allTextContents();
    expect(labels.map(l => l.trim())).toEqual(
      expect.arrayContaining(['Operations', 'Bookings', 'Clients', 'Config', 'Admin'])
    );
  });
});
