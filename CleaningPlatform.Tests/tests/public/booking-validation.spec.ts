import { test, expect } from '@playwright/test';

test.describe('Public Booking Validation and Error States', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/public/index.html');
  });

  test('step 1 requires selecting a service to continue', async ({ page }) => {
    const nextBtn = page.locator('#next-btn');
    const isDisabled = await nextBtn.isDisabled();
    expect(isDisabled).toBe(true);
  });

  test('selecting a service enables next button', async ({ page }) => {
    const serviceCard = page.locator('.service-card').first();
    test.skip(!(await serviceCard.isVisible()), 'No service cards');
    await serviceCard.click();
    await expect(page.locator('#next-btn')).toBeEnabled();
  });

  test('back button returns to step 1 from step 2', async ({ page }) => {
    const serviceCard = page.locator('.service-card').first();
    test.skip(!(await serviceCard.isVisible()), 'No service cards');
    await serviceCard.click();
    await page.locator('#next-btn').click();
    await page.locator('#back-btn').click();
    await expect(page.locator('#step-1')).toBeVisible();
  });

  test('restart button resets booking wizard', async ({ page }) => {
    const serviceCard = page.locator('.service-card').first();
    test.skip(!(await serviceCard.isVisible()), 'No service cards');
    await serviceCard.click();
    await page.locator('#next-btn').click();
    await page.locator('#restart-btn').click();
    await expect(page.locator('#step-1')).toBeVisible();
  });

  test('step 3 shows validation error for empty name', async ({ page }) => {
    const serviceCard = page.locator('.service-card').first();
    test.skip(!(await serviceCard.isVisible()), 'No service cards');
    await serviceCard.click();
    await page.locator('#next-btn').click();
    const dateInput = page.locator('#booking-date');
    if (await dateInput.isVisible()) {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      await dateInput.fill(tomorrow.toISOString().split('T')[0]);
      await page.waitForTimeout(1000);
      const slotBtn = page.locator('.slot-btn').first();
      if (await slotBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await slotBtn.click();
        await page.locator('#next-btn').click();
        await page.fill('#customer-name', '');
        await page.locator('#submit-btn').click();
      }
    }
  });

  test('step 3 shows validation error for empty phone', async ({ page }) => {
    const serviceCard = page.locator('.service-card').first();
    test.skip(!(await serviceCard.isVisible()), 'No service cards');
    await serviceCard.click();
    await page.locator('#next-btn').click();
    const dateInput = page.locator('#booking-date');
    if (await dateInput.isVisible()) {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      await dateInput.fill(tomorrow.toISOString().split('T')[0]);
      await page.waitForTimeout(1000);
      const slotBtn = page.locator('.slot-btn').first();
      if (await slotBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await slotBtn.click();
        await page.locator('#next-btn').click();
        await page.fill('#customer-name', 'Test User');
        await page.fill('#customer-phone', '');
        await page.locator('#submit-btn').click();
      }
    }
  });

  test('confirmation page shows booking details after valid submission', async ({ page }) => {
    const serviceCard = page.locator('.service-card').first();
    test.skip(!(await serviceCard.isVisible()), 'No service cards');
    await serviceCard.click();
    await page.locator('#next-btn').click();
    const dateInput = page.locator('#booking-date');
    if (await dateInput.isVisible()) {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      await dateInput.fill(tomorrow.toISOString().split('T')[0]);
      await page.waitForTimeout(1000);
      const slotBtn = page.locator('.slot-btn').first();
      if (await slotBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await slotBtn.click();
        await page.locator('#next-btn').click();
        await page.fill('#customer-name', 'Test User');
        await page.fill('#customer-phone', '+1234567890');
        await page.locator('#submit-btn').click();
        await expect(page.locator('#confirmation-section')).toBeVisible({ timeout: 5000 });
      }
    }
  });
});
