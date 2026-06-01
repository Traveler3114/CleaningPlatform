import fs from 'node:fs';
import path from 'node:path';

const root = 'C:/Users/matej/Documents/VS Projects/CleaningPlatform/CleaningPlatformAPI';

// ===== STEP 1: Add IStringLocalizer to all managers =====
const managerDir = path.join(root, 'Managers');
const managers = fs.readdirSync(managerDir).filter(f => f.endsWith('.cs'));

for (const file of managers) {
  const fp = path.join(managerDir, file);
  let c = fs.readFileSync(fp, 'utf8');
  
  // Skip if already has IStringLocalizer
  if (c.includes('IStringLocalizer<SharedResources>')) {
    console.log(`  SKIP ${file} (already has localizer)`);
    continue;
  }

  // Add using directive
  if (!c.includes('using Microsoft.Extensions.Localization')) {
    c = c.replace(/(using CleaningPlatformAPI\.Common;)/, 'using Microsoft.Extensions.Localization;\n$1');
    c = c.replace(/(using CleaningPlatformAPI\.Common;)/, 'using CleaningPlatformAPI;\n$1');
  } else if (!c.includes('using CleaningPlatformAPI;')) {
    c = c.replace(/(using CleaningPlatformAPI\.Common;)/, 'using CleaningPlatformAPI;\n$1');
  }

  // Add field
  const className = file.replace('.cs', '');
  c = c.replace(
    new RegExp(`(private readonly \\w+Manager _\\w+Manager;)`),
    `$1\n    private readonly IStringLocalizer<SharedResources> _localizer;`
  );

  // Add constructor parameter - replace closing paren of constructor
  // Pattern: public XxxManager(XxxRepository repo, OtherService svc)
  c = c.replace(
    new RegExp(`(public ${className}\\([^)]*)\\)`),
    `$1, IStringLocalizer<SharedResources> localizer)`
  );

  // Add field assignment in constructor body
  c = c.replace(
    new RegExp(`(public ${className}\\([^)]+\\)\\s*\\{)([^}]*)\\}`),
    (match, start, body) => {
      // Only add assignment if not already present
      if (body.includes('_localizer = localizer')) return match;
      // Find a good place to add - after the last existing assignment in constructor
      const lines = body.split('\n');
      // Add before closing brace of constructor body
      const lastLine = lines.length - 1;
      let insertIdx = lastLine;
      for (let i = lines.length - 1; i >= 0; i--) {
        if (lines[i].trim().startsWith('_')) {
          insertIdx = i + 1;
          break;
        }
      }
      lines.splice(insertIdx, 0, '            _localizer = localizer;');
      return start + lines.join('\n') + '}';
    }
  );

  // Now replace Fail("...") strings with _localizer["key"]
  const msgMap = getMessageMap(file);
  if (msgMap.size > 0) {
    for (const [msg, key] of msgMap) {
      // Match Fail("message") - handle string interpolation and escaping
      const escapedMsg = msg.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
      const regex = new RegExp(`Fail\\(("${escapedMsg}"|'${escapedMsg}')\\)`, 'g');
      c = c.replace(regex, `Fail(_localizer["${key}"])`);
    }
  }

  fs.writeFileSync(fp, c, 'utf8');
  console.log(`  ${file} - updated`);
}

// ===== STEP 2: Update controllers with inline messages =====
const controllerDir = path.join(root, 'Controllers');
const controllers = {
  'AuthController.cs': {
    field: 'private readonly IStringLocalizer<SharedResources> _localizer;',
    ctorParam: 'IStringLocalizer<SharedResources> localizer',
    ctorBody: '_localizer = localizer;',
    replacements: {
      '"Invalid token."': 'error_invalid_token',
    }
  },
  'InvoiceController.cs': {
    field: 'private readonly IStringLocalizer<SharedResources> _localizer;',
    ctorParam: 'IStringLocalizer<SharedResources> localizer',
    ctorBody: '_localizer = localizer;',
    replacements: {
      '"Booking ID is required."': 'err_booking_id_required',
    }
  },
  'PortalAuthController.cs': {
    field: 'private readonly IStringLocalizer<SharedResources> _localizer;',
    ctorParam: 'IStringLocalizer<SharedResources> localizer',
    ctorBody: '_localizer = localizer;',
    replacements: {
      '"Token is required."': 'error_token_required',
    }
  }
};

for (const [file, config] of Object.entries(controllers)) {
  const fp = path.join(controllerDir, file);
  if (!fs.existsSync(fp)) { console.log(`  SKIP ${file} (not found)`); continue; }
  let c = fs.readFileSync(fp, 'utf8');

  if (c.includes('IStringLocalizer<SharedResources>')) {
    console.log(`  SKIP ${file} (already done)`);
    continue;
  }

  // Add usings
  if (!c.includes('using Microsoft.Extensions.Localization')) {
    c = c.replace(/(using CleaningPlatformAPI\.Common;)/, 'using Microsoft.Extensions.Localization;\n$1');
    c = c.replace(/(using CleaningPlatformAPI\.Common;)/, 'using CleaningPlatformAPI;\n$1');
  } else if (!c.includes('using CleaningPlatformAPI;')) {
    c = c.replace(/(using CleaningPlatformAPI\.Common;)/, 'using CleaningPlatformAPI;\n$1');
  }

  // Add field
  const className = file.replace('.cs', '');
  c = c.replace(
    new RegExp(`(private readonly \\w+(Manager|Service) _\\w+[;])`),
    `$1\n    ${config.field}`
  );

  // Add constructor parameter
  c = c.replace(
    new RegExp(`(public ${className}\\([^)]*)\\)`),
    `$1, ${config.ctorParam})`
  );

  // Add field assignment in constructor body
  c = c.replace(
    new RegExp(`(public ${className}\\([^)]+\\)\\s*\\{)([^}]*)\\}`),
    (match, start, body) => {
      if (body.includes('_localizer = localizer')) return match;
      const lines = body.split('\n');
      const lastLine = lines.length - 1;
      let insertIdx = lastLine;
      for (let i = lines.length - 1; i >= 0; i--) {
        if (lines[i].trim().startsWith('_')) {
          insertIdx = i + 1;
          break;
        }
      }
      lines.splice(insertIdx, 0, `            ${config.ctorBody}`);
      return start + lines.join('\n') + '}';
    }
  );

  // Replace messages
  for (const [msg, key] of Object.entries(config.replacements)) {
    const escaped = msg.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    c = c.replace(new RegExp(escaped, 'g'), `_localizer["${key}"]`);
  }

  fs.writeFileSync(fp, c, 'utf8');
  console.log(`  ${file} - updated`);
}

console.log('\nBackend localization complete!');

// ===== Helpers =====
function getMessageMap(managerFile) {
  const map = new Map();
  // Map of manager file -> { "english message": "resx_key" }
  const allMaps = {
    'ServiceCatalogManager.cs': [
      ['Catalog code and name are required.', 'err_catalog_required'],
      ['Service type must be one of: Standard, PerHour, PerItem.', 'err_invalid_service_type'],
      ['Catalog code already exists.', 'err_catalog_code_exists'],
    ],
    'InvoiceManager.cs': [
      ['Booking #{0} was not found.', 'err_booking_not_found'],
      ['Payment amount must be greater than zero.', 'err_payment_amount_zero'],
      ['Invalid payment method.', 'err_invalid_payment_method'],
    ],
    'BookingManager.cs': [
      ['Customer name is required.', 'err_customer_name_required'],
      ['Quantity must be greater than zero.', 'err_quantity_required'],
    ],
    'BookingRequestManager.cs': [
      ['Contact name is required.', 'err_contact_name_required'],
      ['Invalid or expired token.', 'err_invalid_expired_token'],
      ['Invalid name or token.', 'err_invalid_expired_token'],
    ],
    'AuthManager.cs': [
      ['Role is required.', 'err_role_required'],
      ['Password must be at least 8 characters.', 'err_password_length'],
      ['Invalid credentials.', 'err_invalid_credentials'],
    ],
    'SopManager.cs': [
      ['SOP name is required.', 'err_sop_name_required'],
      ['Checklist item text is required.', 'err_checklist_text_required'],
      ['This SOP template is already assigned to this service.', 'err_sop_already_assigned'],
    ],
    'ScheduleManager.cs': [
    ],
    'EmployeeManager.cs': [
    ],
    'ClientManager.cs': [
    ],
    'AvailabilityManager.cs': [
    ],
    'DateOverrideManager.cs': [
    ],
    'RoleManager.cs': [
      ['Role name is required.', 'msg_role_name_required'],
    ],
    'RecurringScheduleManager.cs': [
    ],
    'ReportingManager.cs': [
    ],
    'KanbanManager.cs': [
    ],
    'PortalDataManager.cs': [
    ],
    'TokenManager.cs': [
    ],
  };
  const entries = allMaps[managerFile] || [];
  for (const [msg, key] of entries) {
    map.set(msg, key);
  }
  return map;
}
