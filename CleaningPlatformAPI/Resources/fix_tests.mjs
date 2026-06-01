import fs from 'node:fs';
import path from 'node:path';

const testsRoot = 'C:/Users/matej/Documents/VS Projects/CleaningPlatform/CleaningPlatform.Tests';

// 1. Add Localization package reference to both csproj files
function addPackageRef(csprojPath) {
  let c = fs.readFileSync(csprojPath, 'utf8');
  if (!c.includes('Microsoft.Extensions.Localization')) {
    c = c.replace('</ItemGroup>\n  <ItemGroup>\n    <ProjectReference', 
      '    <PackageReference Include="Microsoft.Extensions.Localization" Version="10.0.8" />\n  </ItemGroup>\n  <ItemGroup>\n    <ProjectReference');
    fs.writeFileSync(csprojPath, c, 'utf8');
    console.log(`  Updated ${path.basename(csprojPath)}`);
  }
}
addPackageRef(path.join(testsRoot, 'Integration', 'CleaningPlatform.Tests.Integration.csproj'));
addPackageRef(path.join(testsRoot, 'Unit', 'CleaningPlatform.Tests.Unit.csproj'));

// 2. Create NullStringLocalizer for Unit tests
const unitNull = `using Microsoft.Extensions.Localization;

namespace CleaningPlatform.Tests.Unit;

public class NullStringLocalizer<T> : IStringLocalizer<T>
{
    public static readonly NullStringLocalizer<T> Instance = new();

    public LocalizedString this[string name] => new(name, name);
    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures = false)
    {
        yield break;
    }
}
`;
fs.writeFileSync(path.join(testsRoot, 'Unit', 'NullStringLocalizer.cs'), unitNull, 'utf8');
console.log('  Created Unit/NullStringLocalizer.cs');

// 3. Update all integration test files to pass NullStringLocalizer<SharedResources>.Instance
const intTestsDir = path.join(testsRoot, 'Integration', 'Tests');
const testFiles = fs.readdirSync(intTestsDir).filter(f => f.endsWith('.cs'));

const managerPatterns = [
  // Pattern: new SomeManager(db) -> new SomeManager(db, NullStringLocalizer<SharedResources>.Instance)
  { from: /new AvailabilityManager\(db\)/g, to: 'new AvailabilityManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new SopManager\(db\)/g, to: 'new SopManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new InvoiceManager\(db\)/g, to: 'new InvoiceManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new ServiceCatalogManager\(db\)/g, to: 'new ServiceCatalogManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new EmployeeManager\(db\)/g, to: 'new EmployeeManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new ClientManager\(db\)/g, to: 'new ClientManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new ScheduleManager\(db\)/g, to: 'new ScheduleManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new DateOverrideManager\(db\)/g, to: 'new DateOverrideManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new RoleManager\(db\)/g, to: 'new RoleManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new KanbanManager\(db\)/g, to: 'new KanbanManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new PortalDataManager\(db\)/g, to: 'new PortalDataManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new ReportingManager\(db\)/g, to: 'new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new RecurringScheduleManager\(db, sop, logger\)/g, to: 'new RecurringScheduleManager(db, sop, logger, NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new TokenManager\(CreateConfiguration\(\)\)/g, to: 'new TokenManager(CreateConfiguration(), NullStringLocalizer<SharedResources>.Instance)' },
  { from: /new TokenManager\(config\)/g, to: 'new TokenManager(config, NullStringLocalizer<SharedResources>.Instance)' },
  // AuthManager: new AuthManager(tokenManager, db, CreateConfiguration())
  { from: /new AuthManager\(tokenManager, db, CreateConfiguration\(\)\)/g, to: 'new AuthManager(tokenManager, db, CreateConfiguration(), NullStringLocalizer<SharedResources>.Instance)' },
  // BookingManager: new BookingManager(db, availability, sop)
  { from: /new BookingManager\(db, availability, sop\)/g, to: 'new BookingManager(db, availability, sop, NullStringLocalizer<SharedResources>.Instance)' },
];

for (const file of testFiles) {
  const fp = path.join(intTestsDir, file);
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;
  
  // Add using for SharedResources
  if (c.includes('NullStringLocalizer<SharedResources>') && !c.includes('using CleaningPlatformAPI;')) {
    c = c.replace(/using CleaningPlatformAPI\.(Common|Managers|Contracts|Entities|Enums);/, 'using CleaningPlatformAPI;\nusing CleaningPlatformAPI.$1;');
  }
  
  for (const { from, to } of managerPatterns) {
    c = c.replace(from, to);
  }
  
  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  ${file} - updated`);
  }
}

// 4. Update unit test TokenManagerTests.cs
const unitTestsDir = path.join(testsRoot, 'Unit', 'Tests');
const unitFiles = fs.readdirSync(unitTestsDir).filter(f => f.endsWith('.cs'));

for (const file of unitFiles) {
  const fp = path.join(unitTestsDir, file);
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;
  
  // Replace TokenManager(config) with TokenManager(config, NullStringLocalizer<...>.Instance)
  c = c.replace(/new TokenManager\(config\)/g, 'new TokenManager(config, NullStringLocalizer<SharedResources>.Instance)');
  c = c.replace(/new TokenManager\(CreateConfig/g, 'new TokenManager(CreateConfig');
  // Add using
  if (c.includes('NullStringLocalizer<SharedResources>') && !c.includes('using CleaningPlatformAPI;')) {
    c = c.replace(/using CleaningPlatformAPI\.(Common|Managers);/, 'using CleaningPlatformAPI;\nusing CleaningPlatformAPI.$1;');
  }
  
  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  Unit/${file} - updated`);
  }
}

console.log('\nTest updates complete!');
