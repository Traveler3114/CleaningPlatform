import { Page } from '@playwright/test';
import jwt from 'jsonwebtoken';
import crypto from 'crypto';
import { BASE_URL, PORTAL_EMAIL, PORTAL_CLIENT_ID, PORTAL_CLIENT_NAME, JWT_SECRET, JWT_ISSUER } from './env';

function createMagicLinkToken(): string {
  return jwt.sign(
    {
      client_id: String(PORTAL_CLIENT_ID),
      email: PORTAL_EMAIL,
      name: PORTAL_CLIENT_NAME,
      auth_type: 'portal',
      purpose: 'magic_link',
      jti: crypto.randomUUID(),
    },
    JWT_SECRET,
    {
      issuer: JWT_ISSUER,
      expiresIn: '15m',
    }
  );
}

export async function loginAsPortalClient(page: Page): Promise<void> {
  const token = createMagicLinkToken();
  const magicLinkUrl = `/portal/magic-link.html?token=${token}`;

  await page.goto(magicLinkUrl);

  await page.waitForURL('**/portal/index.html');
  await page.waitForSelector('.user-pill-name:not(:has-text("Loading..."))');
}

export async function logoutPortalClient(page: Page): Promise<void> {
  await page.evaluate(() => {
    localStorage.removeItem('portalSession');
  });
  await page.goto('/portal/login.html');
}
