using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using tzer0mApi.Services.EInk;

namespace tzer0mApi.Controllers;

/// <summary>
/// Serves display images for the Inky Frame e-ink clock's five button-selectable screens.
/// </summary>
/// <param name="eInkImageService">The service used to render display images.</param>
[ApiController]
[Route("EInk")]
public class EInkController(EInkImageService eInkImageService) : ControllerBase
{
    /// <summary>
    /// Renders the display shown for the given button letter - the clock for A, a coloured placeholder for B-E until they have dedicated content (B red, C yellow, D green, E blue - the four non-black/white inks the Spectra 6 panel can actually produce).
    /// </summary>
    /// <param name="letter">The button letter, A-E.</param>
    /// <returns>An 800x480 PNG image, or 404 if the letter isn't A-E.</returns>
    [HttpGet("{letter}", Name = "EInk Display")]
    public IActionResult GetDisplay(string letter)
    {
        string normalizedLetter = letter.ToUpperInvariant();
        if (normalizedLetter == "A")
            return File(eInkImageService.RenderClock(), "image/png");
        SKColor? colour = normalizedLetter switch
        {
            "B" => SKColors.Red,
            "C" => SKColors.Yellow,
            "D" => SKColors.Lime,
            "E" => SKColors.Blue,
            _ => null
        };
        if (colour is null)
            return NotFound();
        return File(eInkImageService.RenderPlaceholder(normalizedLetter, colour.Value), "image/png");
    }
}