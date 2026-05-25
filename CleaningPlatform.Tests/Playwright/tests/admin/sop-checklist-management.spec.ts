import { test, expect } from '@playwright/test';
import { loginAsAdmin } from '../../helpers/admin-auth';
import { uniqueName } from '../../helpers/data';

test.describe('Admin SOP Checklist Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('SOPs page loads with template list', async ({ page }) => {
    await page.goto('/admin/sops.html');
    await expect(page.locator('.breadcrumb strong')).toContainText('SOP Library');
    await expect(page.locator('#sops-list')).toBeVisible();
  });

  test('create a new SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');
    const templateName = uniqueName('Test SOP');
    await page.fill('#template-name', templateName);
    await page.locator('#create-template-form button[type="submit"]').click();
    await expect(page.locator('#success-container')).toBeVisible({ timeout: 5000 });
    await expect(page.locator(`text=${templateName}`).first()).toBeVisible({ timeout: 5000 });
  });

  test('edit an existing SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');
    await page.waitForSelector('#sops-list table, #sops-list .alert-info', { timeout: 10000 }).catch(() => {});
    const editBtn = page.locator('.sop-row').first();
    test.skip(!(await editBtn.isVisible()), 'No templates exist');
    await editBtn.click();
    await expect(page.locator('#sop-modal')).toBeVisible();
    await page.fill('#edit-template-name', uniqueName('Edited SOP'));
    await page.locator('#edit-template-form button[type="submit"]').click();
  });

  test('deactivate an SOP template', async ({ page }) => {
    await page.goto('/admin/sops.html');
    await page.waitForSelector('#sops-list table, #sops-list .alert-info', { timeout: 10000 }).catch(() => {});
    const deactivateBtn = page.locator('button.btn.btn-sm', { hasText: 'Deactivate' }).first();
    test.skip(!(await deactivateBtn.isVisible()), 'No templates exist');
    await deactivateBtn.click();
  });
});
