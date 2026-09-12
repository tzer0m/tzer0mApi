using Microsoft.AspNetCore.Mvc;
using tzer0mApi.Services.EInk;

namespace tzer0mApi.Controllers;

/// <summary>
/// Serves display images for the Inky Frame e-ink clock.
/// </summary>
/// <param name="eInkImageService">The service used to render display images.</param>
[ApiController]
[Route("EInk")]
public class EInkController(EInkImageService eInkImageService) : ControllerBase
{
    /// <summary>
    /// Renders the clock display shown when button A is selected.
    /// </summary>
    /// <returns>An 800x480 PNG image.</returns>
    [HttpGet("A", Name = "EInk Display A")]
    public IActionResult GetDisplayA()
    {
        byte[] png = eInkImageService.RenderClock();
        return File(png, "image/png");
    }
}