import fs from 'node:fs';
import path from 'node:path';

const root = 'C:/Users/matej/Documents/VS Projects/CleaningPlatform/CleaningPlatformAPI';

// Fix Program.cs - add using CleaningPlatformAPI
let prog = fs.readFileSync(path.join(root, 'Program.cs'), 'utf8');
if (!prog.includes('using CleaningPlatformAPI;')) {
  prog = prog.replace('using CleaningPlatformAPI.Common;', 'using CleaningPlatformAPI;\nusing CleaningPlatformAPI.Common;');
  fs.writeFileSync(path.join(root, 'Program.cs'), prog, 'utf8');
  console.log('Fixed Program.cs - added using CleaningPlatformAPI');
}

// Fix all managers - add field + usings
const managerDir = path.join(root, 'Managers');
const files = fs.readdirSync(managerDir).filter(f => f.endsWith('.cs'));

for (const file of files) {
  const fp = path.join(managerDir, file);
  let c = fs.readFileSync(fp, 'utf8');

  // 1. Add usings if missing
  if (!c.includes('using Microsoft.Extensions.Localization;')) {
    c = c.replace(/using CleaningPlatformAPI\.Common;/, 'using Microsoft.Extensions.Localization;\nusing CleaningPlatformAPI.Common;');
  }
  if (!c.includes('using CleaningPlatformAPI;')) {
    c = c.replace(/using CleaningPlatformAPI\.Common;/, 'using CleaningPlatformAPI;\nusing CleaningPlatformAPI.Common;');
  }

  // 2. Add field if missing
  if (!c.includes('IStringLocalizer<SharedResources> _localizer')) {
    // Find the last field declaration before the constructor
    const fieldMatch = c.match(/^\s+private readonly\s+\S+\s+\S+;/m);
    if (fieldMatch) {
      c = c.replace(fieldMatch[0], fieldMatch[0] + '\n    private readonly IStringLocalizer<SharedResources> _localizer;');
    } else {
      // Try to find another pattern
      const staticMatch = c.match(/^\s+private static/);
      if (staticMatch) {
        c = c.replace(staticMatch[0], '    private readonly IStringLocalizer<SharedResources> _localizer;\n' + staticMatch[0]);
      }
    }
  }

  // 3. Fix constructor - ensure _localizer = localizer is after field
  const clsName = file.replace('.cs', '');
  if (!c.includes('_localizer = localizer') && c.includes('IStringLocalizer<SharedResources> localizer')) {
    // Find constructor opening brace and add assignment
    c = c.replace(
      new RegExp(`(public ${clsName}\\([^)]+\\)\\s*\\{)`),
      (match) => {
        // Check if body already has assignments ending with ;
        const after = c.substring(c.indexOf(match) + match.length);
        if (!after.trim().startsWith('_localizer = localizer')) {
          return match + '\n            _localizer = localizer;';
        }
        return match;
      }
    );
  }

  // 4. Fix case where constructor body is on one line like: { _db = db; _localizer = localizer; }
  c = c.replace(/=\s*localizer;\s*\n?\s*_localizer = localizer\s*/g, '= localizer;\n            _localizer = localizer');

  // Clean up any double assignments
  c = c.replace(/_localizer = localizer;\s*\n?\s*_localizer = localizer;/g, '_localizer = localizer;');

  fs.writeFileSync(fp, c, 'utf8');
  console.log(`  Fixed ${file}`);
}

// Fix controller usings
const controllerDir = path.join(root, 'Controllers');
const controllerFiles = ['AuthController.cs', 'InvoiceController.cs', 'PortalAuthController.cs'];
for (const file of controllerFiles) {
  const fp = path.join(controllerDir, file);
  if (!fs.existsSync(fp)) continue;
  let c = fs.readFileSync(fp, 'utf8');
  if (!c.includes('using Microsoft.Extensions.Localization') && c.includes('IStringLocalizer')) {
    c = c.replace(/using CleaningPlatformAPI\.Common;/, 'using Microsoft.Extensions.Localization;\nusing CleaningPlatformAPI.Common;');
  }
  if (!c.includes('using CleaningPlatformAPI;') && c.includes('SharedResources')) {
    c = c.replace(/using CleaningPlatformAPI\.Common;/, 'using CleaningPlatformAPI;\nusing CleaningPlatformAPI.Common;');
  }
  fs.writeFileSync(fp, c, 'utf8');
  console.log(`  Fixed ${file} usings`);
}

console.log('Done!');
