using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using System.Resources;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/localization")]
public class LocalizationController : ControllerBase
{
    private readonly IStringLocalizer<SharedResources> _localizer;

    public LocalizationController(IStringLocalizer<SharedResources> localizer)
    {
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string? culture)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo(culture);
        }

        var result = new Dictionary<string, string>();
        try
        {
            foreach (var entry in _localizer.GetAllStrings())
            {
                result[entry.Name] = entry.Value;
            }
        }
        catch (MissingManifestResourceException)
        {
            // No .resx files exist — return empty set, frontend will use embedded fallbacks
        }
        return Ok(result);
    }
}
