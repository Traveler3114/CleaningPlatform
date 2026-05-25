# All 3 test projects (from repo root)
dotnet test CleaningPlatform.Tests\Unit
dotnet test CleaningPlatform.Tests\Integration
npx playwright test --config CleaningPlatform.Tests\Playwright\playwright.config.ts