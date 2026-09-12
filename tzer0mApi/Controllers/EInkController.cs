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
    /// Renders the display shown for the given button letter - the clock for A, a placeholder for B-E until they have dedicated content.
    /// </summary>
    /// <param name="letter">The button letter, A-E.</param>
    /// <returns>An 800x480 PNG image, or 404 if the letter isn't A-E.</returns>
    [HttpGet("{letter}", Name = "EInk Display")]
    public IActionResult GetDisplay(string letter)
    {
        string normalizedLetter = letter.ToUpperInvariant();
        if (normalizedLetter == "A")
            return File(eInkImageService.RenderClock(), "image/png");
        if (normalizedLetter is "B" or "C" or "D" or "E")
            return File(eInkImageService.RenderPlaceholder(normalizedLetter), "image/png");
        return NotFound();
    }

    /// <summary>
    /// Renders a solid colour swatch for the given button letter, for testing the panel's colour rendering - A is red, B orange, C yellow, D green, and E blue.
    /// </summary>
    /// <param name="letter">The button letter, A-E.</param>
    /// <returns>An 800x480 PNG image, or 404 if the letter isn't A-E.</returns>
    [HttpGet("ColourTest/{letter}", Name = "EInk Colour Test")]
    public IActionResult GetColourTest(string letter)
    {
        string normalizedLetter = letter.ToUpperInvariant();
        SKColor? colour = normalizedLetter switch
        {
            "A" => SKColors.Red,
            "B" => SKColors.Orange,
            "C" => SKColors.Yellow,
            "D" => SKColors.Green,
            "E" => SKColors.Blue,
            _ => null
        };
        if (colour is null)
            return NotFound();
        return File(eInkImageService.RenderColourSwatch(normalizedLetter, colour.Value), "image/png");
    }
}