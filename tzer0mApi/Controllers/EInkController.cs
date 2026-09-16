using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using tzer0mApi.Models.HomeAssistant;
using tzer0mApi.Models.Kuma;
using tzer0mApi.Services.EInk;
using tzer0mApi.Services.HomeAssistant;
using tzer0mApi.Services.Kuma;

namespace tzer0mApi.Controllers;

/// <summary>
/// Serves display images for the Inky Frame e-ink clock's five button-selectable screens.
/// </summary>
/// <param name="eInkImageService">The service used to render display images.</param>
/// <param name="homeAssistantService">The service used to fetch calendar, task, and weather data for display A.</param>
/// <param name="kumaService">The service used to fetch monitor status for display B.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("EInk")]
public class EInkController(EInkImageService eInkImageService, HomeAssistantService homeAssistantService, KumaService kumaService, ILogger<EInkController> logger) : ControllerBase
{
    /// <summary>
    /// Renders the display shown for the given button letter - the home screen for A, the Kuma status board for B, a coloured placeholder for C-E until they have dedicated content (C yellow, D green, E blue - the remaining non-black/white inks the Spectra 6 panel can actually produce).
    /// </summary>
    /// <param name="letter">The button letter, A-E.</param>
    /// <returns>An 800x480 PNG image, or 404 if the letter isn't A-E.</returns>
    [HttpGet("{letter}", Name = "EInk Display")]
    public async Task<IActionResult> GetDisplay(string letter)
    {
        string normalizedLetter = letter.ToUpperInvariant();
        if (normalizedLetter == "A")
            return File(await RenderHomeScreenAsync(), "image/png");
        if (normalizedLetter == "B")
            return File(await RenderKumaStatusAsync(), "image/png");
        SKColor? colour = normalizedLetter switch
        {
            "C" => SKColors.Yellow,
            "D" => SKColors.Lime,
            "E" => SKColors.Blue,
            _ => null
        };
        if (colour is null)
            return NotFound();
        return File(eInkImageService.RenderPlaceholder(normalizedLetter, colour.Value), "image/png");
    }

    /// <summary>
    /// Fetches today's weather, events, and tasks from Home Assistant and renders the home screen - a source that fails to fetch is simply omitted rather than taking down the whole display.
    /// </summary>
    private async Task<byte[]> RenderHomeScreenAsync()
    {
        HomeAssistantWeather? weather = await TryGetAsync(homeAssistantService.GetWeatherAsync, "weather from Home Assistant");
        List<HomeAssistantEvent> events = await TryGetAsync(homeAssistantService.GetTodaysEventsAsync, "events from Home Assistant") ?? [];
        List<HomeAssistantTask> tasks = await TryGetAsync(homeAssistantService.GetOverdueAndDueTodayTasksAsync, "tasks from Home Assistant") ?? [];
        return eInkImageService.RenderHomeScreen(DateTime.Now, weather, events, tasks);
    }

    /// <summary>
    /// Fetches Kuma's status summary and renders the status board - a failed fetch renders a simple unavailable message rather than taking down the whole display.
    /// </summary>
    private async Task<byte[]> RenderKumaStatusAsync()
    {
        KumaStatusSummary? summary = await TryGetAsync(kumaService.GetStatusSummaryAsync, "status from Kuma");
        return eInkImageService.RenderKumaStatus(summary);
    }

    /// <summary>
    /// Runs the given fetch, logging and returning the default value if it fails rather than taking down the whole display.
    /// </summary>
    /// <typeparam name="T">The fetch's result type.</typeparam>
    /// <param name="fetch">The fetch to run.</param>
    /// <param name="sourceName">A short description of the data source, used in the log message.</param>
    private async Task<T?> TryGetAsync<T>(Func<Task<T>> fetch, string sourceName)
    {
        try
        {
            return await fetch();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not fetch {SourceName}", sourceName);
            return default;
        }
    }
}