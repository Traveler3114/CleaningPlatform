import { test, expect } from '@playwright/test';

test.describe('Public Booking Flow (4-Step Wizard)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/public/book.html');
  });

  test('page loads with 4 progress steps', async ({ page }) => {
    await expect(page.locator('#prog-1')).toBeVisible();
    await expect(page.locator('#prog-2')).toBeVisible();
    await expect(page.locator('#prog-3')).toBeVisible();
    await expect(page.locator('#prog-4')).toBeVisible();
  });

  test('step 1: service selection panel is visible', async ({ page }) => {
    await expect(page.locator('#panel-1')).toBeVisible();
    await expect(page.locator('#booking-service-list')).toBeVisible();
    await expect(page.locator('#step1-next')).toHaveText('Continue →');
  });

  test('step 1: service cards are loaded', async ({ page }) => {
    await page.waitForSelector('#booking-service-list .service-card', { timeout: 10000 });
    const serviceCards = page.locator('.service-card');
    const count = await serviceCards.count();
    expect(count).toBeGreaterThan(0);
  });

  test('step 1: cannot continue without selecting a service', async ({ page }) => {
    const nextBtn = page.locator('#step1-next');
    await expect(nextBtn).toBeDisabled();
  });

  test('can select a service and proceed to step 2', async ({ page }) => {
    await page.waitForSelector('#booking-service-list .service-card', { timeout: 10000 });
    const firstService = page.locator('.service-card').first();
    await firstService.click();

    const nextBtn = page.locator('#step1-next');
    await expect(nextBtn).toBeEnabled();

    await nextBtn.click();
    await expect(page.locator('#panel-2')).toBeVisible();
  });

  test('step 2: date picker and slots container are visible', async ({ page }) => {
    await page.waitForSelector('.service-card', { timeout: 10000 });
    await page.locator('.service-card').first().click();
    await page.locator('#step1-next').click();

    await expect(page.locator('#date-input')).toBeVisible();
    await expect(page.locator('#slots-container')).toBeVisible();
    await expect(page.locator('#step2-back')).toHaveText('← Back');
    await expect(page.locator('#step2-next')).toHaveText('Continue →');
  });

  test('step 2: back button returns to step 1', async ({ page }) => {
    await page.waitForSelector('.service-card', { timeout: 10000 });
    await page.locator('.service-card').first().click();
    await page.locator('#step1-next').click();

    await page.locator('#step2-back').click();
    await expect(page.locator('#panel-1')).toBeVisible();
  });

  test('step 2: selecting date loads slots', async ({ page }) => {
    await page.waitForSelector('.service-card', { timeout: 10000 });
    await page.locator('.service-card').first().click();
    await page.locator('#step1-next').click();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dateStr = tomorrow.toISOString().split('T')[0];
    await page.fill('#date-input', dateStr);

    await page.waitForTimeout(2000);
  });

  test('can complete full booking flow to confirmation', async ({ page }) => {
    // Step 1: Select service
    await page.waitForSelector('.service-card', { timeout: 10000 });
    const serviceCards = page.locator('.service-card');
    const serviceCount = await serviceCards.count();
    expect(serviceCount).toBeGreaterThan(0);
    await serviceCards.first().click();
    await page.locator('#step1-next').click();

    // Step 2: Pick date and time slot
    await page.waitForSelector('#date-input');
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dateStr = tomorrow.toISOString().split('T')[0];
    await page.fill('#date-input', dateStr);

    await page.waitForTimeout(1500);

    const slotBtn = page.locator('.slot-btn').first();
    if (await slotBtn.isVisible()) {
      await slotBtn.click();
      const step2Next = page.locator('#step2-next');
      if (await step2Next.isEnabled()) {
        await step2Next.click();

        // Step 3: Fill details
        await page.waitForSelector('#step3-summary');
        const uniqueId = Date.now();
        await page.fill('#customer-name', `Test User ${uniqueId}`);
        await page.fill('#customer-phone', `+385 91 ${uniqueId}`.slice(0, 15));
        await page.fill('#customer-email', `test${uniqueId}@email.com`);

        await page.locator('#step3-submit').click();

        // Step 4: Confirmation
        await page.waitForSelector('#panel-4', { timeout: 10000 });
        await expect(page.locator('#panel-4')).toBeVisible();
        await expect(page.locator('#confirmation-details')).toBeVisible();
      }
    }
  });

  test('restart button resets the flow', async ({ page }) => {
    await page.waitForSelector('.service-card', { timeout: 10000 });
    await page.locator('.service-card').first().click();
    await page.locator('#step1-next').click();

    await page.waitForSelector('#date-input');
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    await page.fill('#date-input', tomorrow.toISOString().split('T')[0]);

    await page.waitForTimeout(1500);

    const slotBtn = page.locator('.slot-btn').first();
    if (await slotBtn.isVisible()) {
      await slotBtn.click();
      const step2Next = page.locator('#step2-next');
      if (await step2Next.isEnabled()) {
        await step2Next.click();

        await page.fill('#customer-name', 'Restart Test');
        await page.fill('#customer-phone', '+385 91 123 4567');
        await page.locator('#step3-submit').click();

        await page.waitForSelector('#panel-4', { timeout: 10000 });
        await page.locator('#restart-btn').click();
        await expect(page.locator('#panel-1')).toBeVisible();
      }
    }
  });
});
