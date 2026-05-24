import { PORTAL_EMAIL, PORTAL_CLIENT_ID, PORTAL_CLIENT_NAME, JWT_SECRET, JWT_ISSUER } from './env';

const TS = Date.now();

export function uniqueName(prefix: string): string {
  return `${prefix} ${new Date(TS).toISOString().replace(/[:.]/g, '-')}`;
}

export function uniqueEmail(prefix: string): string {
  return `${prefix.toLowerCase()}-${TS}@test.com`;
}

export function uniquePhone(): string {
  return `+1${String(TS).slice(-10)}`;
}

export function futureDate(daysAhead: number): string {
  const d = new Date(TS);
  d.setDate(d.getDate() + daysAhead);
  return d.toISOString().split('T')[0];
}

export { PORTAL_EMAIL, PORTAL_CLIENT_ID, PORTAL_CLIENT_NAME, JWT_SECRET, JWT_ISSUER };
