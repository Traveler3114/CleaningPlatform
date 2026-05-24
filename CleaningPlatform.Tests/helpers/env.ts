import { config } from 'dotenv';
import path from 'path';

config({ path: path.resolve(__dirname, '..', '.env') });

export const BASE_URL = process.env.BASE_URL || 'https://localhost:7124';
export const ADMIN_USERNAME = process.env.ADMIN_USERNAME || 'owner';
export const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'ChangeMe123!';
export const PORTAL_EMAIL = process.env.PORTAL_EMAIL || 'contact1@client.com';
export const PORTAL_CLIENT_ID = Number(process.env.PORTAL_CLIENT_ID) || 1;
export const PORTAL_CLIENT_NAME = process.env.PORTAL_CLIENT_NAME || 'Contact 1';
export const JWT_SECRET = process.env.JWT_SECRET || '-hjY2CRcM@i(N&]C9DZSnW|QPrBz&mM!S9sU}hU%cj4Z]Be9*d!QcRH7!uu&kQ8H+A,cS,^c;qGZ1!^b@@d#*m';
export const JWT_ISSUER = process.env.JWT_ISSUER || 'CleaningPlatform';
