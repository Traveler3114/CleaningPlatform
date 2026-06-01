import fs from 'node:fs';
import path from 'node:path';

const dir = 'C:/Users/matej/Documents/VS Projects/CleaningPlatform/CleaningPlatform.Tests/Integration/Tests';
const files = fs.readdirSync(dir).filter(f => f.endsWith('.cs'));

for (const file of files) {
  const fp = path.join(dir, file);
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;

  // Add using CleaningPlatformAPI; after the first using line that starts with "using CleaningPlatformAPI."
  if (c.includes('NullStringLocalizer<SharedResources>') && !c.includes('using CleaningPlatformAPI;')) {
    c = c.replace(/^(using CleaningPlatformAPI\.\w+;)/m, 'using CleaningPlatformAPI;\n$1');
  }

  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  Fixed ${file}`);
  }
}
console.log('Done!');
