import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { uniqueName } from '../../helpers/data';

test.describe('Admin SOP Checklist Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('SOPs page loads with template list', async ({ page }) => {
    await page.goto('/admin/sops.html');
    await expect(page.locator('h1')).toContainText('SOP Library');
    await expect(page.locator('#template-list')).toBeVisible();
  });

  test('create a new SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');
    const templateName = uniqueName('Test SOP');
    await page.locator('#create-template-btn').click();
    await page.fill('#template-name', templateName);
    await page.locator('#save-template-btn').click();
    await expect(page.locator('#success-message', { hasText: 'success' }).first()).toBeVisible({ timeout: 5000 });
    await expect(page.locator(`text=${templateName}`).first()).toBeVisible({ timeout: 5000 });
  });

  test('edit an existing SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');
    const editBtn = page.locator('.edit-template-btn').first();
    test.skip(!(await editBtn.isVisible()), 'No templates exist');
    await editBtn.click();
    await expect(page.locator('#edit-template-modal')).toBeVisible();
    await page.fill('#template-name', uniqueName('Edited SOP'));
    await page.locator('#save-edit-btn').click();
  });

  test('deactivate an SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');
    const deactivateBtn = page.locator('.deactivate-btn').first();
    test.skip(!(await deactivateBtn.isVisible()), 'No templates exist');
    await deactivateBtn.click();
  });
});
