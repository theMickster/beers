import { defineConfig } from '@playwright/test';

const baseURL = process.env.BASE_URL ?? 'http://localhost:8080';

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  retries: process.env.CI ? 1 : 0,
  timeout: 30_000,
  use: {
    baseURL,
    extraHTTPHeaders: {
      Accept: 'application/json'
    }
  },
  reporter: process.env.CI ? 'line' : 'list'
});
