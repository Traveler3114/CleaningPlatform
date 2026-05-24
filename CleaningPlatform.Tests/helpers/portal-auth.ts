import { Page } from '@playwright/test';
import { SignJWT } from 'jose';
import crypto from 'crypto';
import { PORTAL_EMAIL, PORTAL_CLIENT_ID, PORTAL_CLIENT_NAME, JWT_SECRET, JWT_ISSUER } from './env';

export async function loginAsPortalClient(page: Page): Promise<void> {
  const response = await page.request.post('/api/portal/validate-token', {
    data: {
      token: await createMagicLinkToken(),
    },
  });

  const result = await response.json();
  if (!result.success || !result.data) {
    throw new Error(`Portal auth failed: ${result.message || 'Unknown error'}`);
  }

  await page.goto('/portal/login.html');
  await page.evaluate((sessionToken: string) => {
    localStorage.setItem('portalSession', sessionToken);
  }, result.data);

  await page.goto('/portal/index.html');
  await page.waitForSelector('.user-pill-name:not(:has-text("Loading..."))');
}

export async function logoutPortalClient(page: Page): Promise<void> {
  await page.evaluate(() => {
    localStorage.removeItem('portalSession');
  });
  await page.goto('/portal/login.html');
}

async function createMagicLinkToken(): Promise<string> {
  return new SignJWT({
    client_id: String(PORTAL_CLIENT_ID),
    email: PORTAL_EMAIL,
    name: PORTAL_CLIENT_NAME,
    auth_type: 'portal',
    purpose: 'magic_link',
  })
    .setProtectedHeader({ alg: 'HS256' })
    .setIssuer(JWT_ISSUER)
    .setExpirationTime('15m')
    .setJti(crypto.randomUUID())
    .sign(new TextEncoder().encode(JWT_SECRET));
}
