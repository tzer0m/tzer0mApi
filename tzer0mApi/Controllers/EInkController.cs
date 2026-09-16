using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using tzer0mApi.Models.HomeAssistant;
using tzer0mApi.Models.Kuma;
using tzer0mApi.Models.Rss;
using tzer0mApi.Models.SmarterMeter;
using tzer0mApi.Services.EInk;
using tzer0mApi.Services.HomeAssistant;
using tzer0mApi.Services.Kuma;
using tzer0mApi.Services.Rss;
using tzer0mApi.Services.SmarterMeter;

namespace tzer0mApi.Controllers;

/// <summary>
/// Serves display images for the Inky Frame e-ink clock's five button-selectable screens.
/// </summary>
/// <param name="eInkImageService">The service used to render display images.</param>
/// <param name="homeAssistantService">The service used to fetch calendar, task, and weather data for display A.</param>
/// <param name="kumaService">The service used to fetch monitor status for display B.</param>
/// <param name="databaseService">The service used to fetch meter readings for display C.</param>
/// <param name="calculationService">The service used to calculate usage and cost for display C.</param>
/// <param name="rssService">The service used to fetch and parse the RSS feed for display D.</param>
/// <param name="config">Configuration, used to resolve the SmarterMeter capture interval.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("EInk")]
public class EInkController(EInkImageService eInkImageService, HomeAssistantService homeAssistantService, KumaService kumaService, DatabaseService databaseService, CalculationService calculationService, RssService rssService, IConfiguration config, ILogger<EInkController> logger) : ControllerBase
{
    /// <summary>
    /// Renders the display shown for the given button letter - the home screen for A, the Kuma status board for B, the SmarterMeter status board for C, the RSS feed for D, a coloured placeholder for E until it has dedicated content (blue - the remaining non-black/white ink the Spectra 6 panel can actually produce).
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
        if (normalizedLetter == "C")
            return File(await RenderMeterSummaryAsync(), "image/png");
        if (normalizedLetter == "D")
            return File(await RenderRssFeedAsync(), "image/png");
        SKColor? colour = normalizedLetter switch
        {
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
    /// Builds a usage and cost summary from the SmarterMeter database and renders the status board - a failed fetch renders a simple unavailable message rather than taking down the whole display.
    /// </summary>
    private async Task<byte[]> RenderMeterSummaryAsync()
    {
        MeterSummary? summary = await TryGetAsync(BuildMeterSummaryAsync, "summary from SmarterMeter");
        return eInkImageService.RenderMeterSummary(summary);
    }

    /// <summary>
    /// Fetches recent meter readings and calculates the usage and cost summary, matching the logic behind the /SmarterMeter/Summary endpoint.
    /// </summary>
    private async Task<MeterSummary> BuildMeterSummaryAsync()
    {
        int captureIntervalHours = config.GetValue<int?>("SmarterMeter:CaptureIntervalHours") ?? 1;
        const int expectedReadings = 100;
        int lookbackHours = expectedReadings * captureIntervalHours;
        List<MeterReading> readings = [.. await databaseService.GetRecentReadingsAsync(500)];
        DateTime cutoff = DateTime.UtcNow.AddHours(-lookbackHours);
        decimal successRate = Math.Round(Math.Min(readings.Count(reading => reading.CapturedAt >= cutoff) / (decimal)expectedReadings * 100m, 100m), 1);
        return calculationService.Calculate(readings, successRate, captureIntervalHours);
    }

    /// <summary>
    /// Fetches the RSS feed's items and renders the feed list - a failed fetch renders a simple unavailable message rather than taking down the whole display.
    /// </summary>
    private async Task<byte[]> RenderRssFeedAsync()
    {
        List<RssFeedItem>? items = await TryGetAsync(rssService.GetItemsAsync, "items from the RSS feed");
        return eInkImageService.RenderRssFeed(items);
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