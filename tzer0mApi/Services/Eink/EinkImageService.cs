using System.IO.Compression;
using System.Linq;
using System.Text;
using SkiaSharp;
using tzer0mApi.Models.HomeAssistant;
using tzer0mApi.Models.Kuma;
using tzer0mApi.Models.Rss;
using tzer0mApi.Models.SmarterMeter;

namespace tzer0mApi.Services.EInk;

/// <summary>
/// Renders display images for the Inky Frame e-ink clock.
/// </summary>
/// <param name="env">The web host environment, used to resolve the bundled font files' paths.</param>
public class EInkImageService(IWebHostEnvironment env)
{
    /// <summary>
    /// The display width, in pixels, matching the Inky Frame 7.3" panel.
    /// </summary>
    private const int WidthPx = 800;

    /// <summary>
    /// The display height, in pixels, matching the Inky Frame 7.3" panel.
    /// </summary>
    private const int HeightPx = 480;

    /// <summary>
    /// Font size, in points, used for the time.
    /// </summary>
    private const float TimeFontSize = 160f;

    /// <summary>
    /// Font size, in points, used for the date.
    /// </summary>
    private const float DateFontSize = 40f;

    /// <summary>
    /// Font size, in points, used for a placeholder display's big letter.
    /// </summary>
    private const float PlaceholderLetterFontSize = 260f;

    /// <summary>
    /// Font size, in points, used for a placeholder display's caption.
    /// </summary>
    private const float PlaceholderCaptionFontSize = 40f;

    /// <summary>
    /// Left/right margin used throughout the home screen.
    /// </summary>
    private const float HomeMarginPx = 32f;

    /// <summary>
    /// Font size, in points, used for the home screen's hero day-of-month number.
    /// </summary>
    private const float DayNumberFontSize = 128f;

    /// <summary>
    /// Baseline y-position for the home screen's hero day-of-month number.
    /// </summary>
    private const float DayNumberBaselineY = 129f;

    /// <summary>
    /// Gap, in pixels, between the hero day number and the weekday/month block beside it.
    /// </summary>
    private const float HeaderDateGapPx = 22f;

    /// <summary>
    /// Font size, in points, used for the home screen's weekday and month.
    /// </summary>
    private const float WeekdayFontSize = 34f;

    /// <summary>
    /// Baseline y-position for the home screen's weekday line.
    /// </summary>
    private const float WeekdayBaselineY = 66f;

    /// <summary>
    /// Baseline y-position for the home screen's month line.
    /// </summary>
    private const float MonthBaselineY = 108f;

    /// <summary>
    /// Font size, in points, used for the home screen's time.
    /// </summary>
    private const float HomeTimeFontSize = 60f;

    /// <summary>
    /// Baseline y-position for the home screen's time.
    /// </summary>
    private const float HomeTimeBaselineY = 92f;

    /// <summary>
    /// Baseline y-position for the home screen's weather row.
    /// </summary>
    private const float WeatherRowBaselineY = 138f;

    /// <summary>
    /// Gap, in pixels, between the weather icon, temperature, and condition label.
    /// </summary>
    private const float WeatherRowGapPx = 14f;

    /// <summary>
    /// Radius, in pixels, of the weather icon.
    /// </summary>
    private const float WeatherIconRadiusPx = 20f;

    /// <summary>
    /// Font size, in points, used for the weather temperature.
    /// </summary>
    private const float WeatherTempFontSize = 30f;

    /// <summary>
    /// Font size, in points, used for the weather condition label.
    /// </summary>
    private const float WeatherLabelFontSize = 20f;

    /// <summary>
    /// Y-position of the divider line under the home screen's header.
    /// </summary>
    private const float HeaderDividerY = 158f;

    /// <summary>
    /// Thickness, in pixels, of a divider line.
    /// </summary>
    private const float DividerThicknessPx = 3f;

    /// <summary>
    /// Gap, in pixels, between the header divider and the start of the body columns.
    /// </summary>
    private const float BodyTopPaddingPx = 24f;

    /// <summary>
    /// Margin kept clear at the bottom of the body columns.
    /// </summary>
    private const float BodyBottomPaddingPx = 26f;

    /// <summary>
    /// Font size, in points, used for the "TODAY" and "TASKS" section headers.
    /// </summary>
    private const float SectionHeaderFontSize = 20f;

    /// <summary>
    /// Gap, in pixels, between a section header and its first row.
    /// </summary>
    private const float SectionHeaderGapPx = 40f;

    /// <summary>
    /// Gap, in pixels, either side of the vertical divider between the two body columns.
    /// </summary>
    private const float ColumnGapPx = 32f;

    /// <summary>
    /// Font size, in points, used for an event's time and title.
    /// </summary>
    private const float EventFontSize = 22f;

    /// <summary>
    /// Width, in pixels, reserved for an event's time column.
    /// </summary>
    private const float EventTimeColumnWidthPx = 90f;

    /// <summary>
    /// Fraction of the body's width given to the all-day events column.
    /// </summary>
    private const float AllDayColumnFraction = 0.25f;

    /// <summary>
    /// Fraction of the body's width given to the timed events column - the remainder goes to the tasks column.
    /// </summary>
    private const float EventsColumnFraction = 0.50f;

    /// <summary>
    /// Space, in pixels, reserved between a column's text and the divider that follows it.
    /// </summary>
    private const float ColumnTextTrailingPaddingPx = 24f;

    /// <summary>
    /// Vertical distance, in pixels, between two wrapped lines of the same list item.
    /// </summary>
    private const float WrapLineHeightPx = 26f;

    /// <summary>
    /// Vertical gap, in pixels, left after one list item before the next one starts.
    /// </summary>
    private const float ItemGapPx = 14f;

    /// <summary>
    /// Extra vertical padding, in pixels, added above/below the text when drawing a current event's highlight background.
    /// </summary>
    private const float EventHighlightPaddingPx = 6f;

    /// <summary>
    /// Horizontal inset, in pixels, of a current event's highlight background from the row's left margin.
    /// </summary>
    private const float EventHighlightHorizontalPaddingPx = 10f;

    /// <summary>
    /// Space, in pixels, between a current event's highlight background and the divider that follows it.
    /// </summary>
    private const float EventHighlightTrailingPaddingPx = 16f;

    /// <summary>
    /// Corner radius, in pixels, of a current event's highlight background.
    /// </summary>
    private const float EventHighlightCornerRadiusPx = 8f;

    /// <summary>
    /// Thickness, in pixels, of the strikethrough line drawn through a past event.
    /// </summary>
    private const float EventStrikeThicknessPx = 2f;

    /// <summary>
    /// Font size, in points, used for a task's "OVERDUE" label.
    /// </summary>
    private const float TaskLabelFontSize = 15f;

    /// <summary>
    /// Vertical gap, in pixels, between an overdue task's "OVERDUE" label and its title.
    /// </summary>
    private const float TaskOverdueLabelGapPx = 20f;

    /// <summary>
    /// Font size, in points, used for the Kuma status board's overall status line.
    /// </summary>
    private const float KumaStatusLineFontSize = 32f;

    /// <summary>
    /// Baseline y-position, in pixels, of the Kuma status board's overall status line.
    /// </summary>
    private const float KumaStatusLineBaselineY = 60f;

    /// <summary>
    /// Radius, in pixels, of the Kuma status board's overall status icon.
    /// </summary>
    private const float KumaStatusIconRadiusPx = 18f;

    /// <summary>
    /// Gap, in pixels, between the Kuma status board's overall status icon and its text.
    /// </summary>
    private const float KumaStatusIconTextGapPx = 16f;

    /// <summary>
    /// Y-position, in pixels, of the divider below the Kuma status board's overall status line.
    /// </summary>
    private const float KumaDividerY = 78f;

    /// <summary>
    /// Width, in pixels, of the Kuma status board's host name column.
    /// </summary>
    private const float KumaHostColumnWidthPx = 160f;

    /// <summary>
    /// Width, in pixels, of the Kuma status board's uptime column.
    /// </summary>
    private const float KumaUptimeColumnWidthPx = 110f;

    /// <summary>
    /// Width, in pixels, of the Kuma status board's up-count column.
    /// </summary>
    private const float KumaUpColumnWidthPx = 70f;

    /// <summary>
    /// Width, in pixels, of the Kuma status board's total-monitor-count column.
    /// </summary>
    private const float KumaTotalColumnWidthPx = 70f;

    /// <summary>
    /// Height, in pixels, of a tick in the Kuma status board's recent-checks history.
    /// </summary>
    private const float KumaTickHeightPx = 20f;

    /// <summary>
    /// Stroke thickness, in pixels, of a tick in the Kuma status board's recent-checks history.
    /// </summary>
    private const float KumaTickThicknessPx = 4f;

    /// <summary>
    /// Target horizontal spacing, in pixels, between ticks in the Kuma status board's recent-checks history - used to work out how many ticks fit.
    /// </summary>
    private const float KumaTickSpacingPx = 8f;

    /// <summary>
    /// Height, in pixels, of each of the SmarterMeter board's three top boxes (current reading, last read, success rate).
    /// </summary>
    private const float MeterTopBoxHeightPx = 76f;

    /// <summary>
    /// Gap, in pixels, between the SmarterMeter board's three top boxes.
    /// </summary>
    private const float MeterTopBoxGapPx = 16f;

    /// <summary>
    /// Corner radius, in pixels, of the SmarterMeter board's top boxes.
    /// </summary>
    private const float MeterTopBoxCornerRadiusPx = 10f;

    /// <summary>
    /// Relative width unit of the SmarterMeter board's current-reading box, against <see cref="MeterLastReadBoxWidthUnits"/> and <see cref="MeterSuccessBoxWidthUnits"/>.
    /// </summary>
    private const float MeterReadingBoxWidthUnits = 2f;

    /// <summary>
    /// Relative width unit of the SmarterMeter board's last-read box, against <see cref="MeterReadingBoxWidthUnits"/> and <see cref="MeterSuccessBoxWidthUnits"/>.
    /// </summary>
    private const float MeterLastReadBoxWidthUnits = 2f;

    /// <summary>
    /// Relative width unit of the SmarterMeter board's success-rate box, against <see cref="MeterReadingBoxWidthUnits"/> and <see cref="MeterLastReadBoxWidthUnits"/>.
    /// </summary>
    private const float MeterSuccessBoxWidthUnits = 1f;

    /// <summary>
    /// Font size, in points, used for the SmarterMeter board's current reading.
    /// </summary>
    private const float MeterReadingFontSize = 40f;

    /// <summary>
    /// Font size, in points, used for the SmarterMeter board's "kWh" unit suffix.
    /// </summary>
    private const float MeterReadingUnitFontSize = 18f;

    /// <summary>
    /// Font size, in points, used for the SmarterMeter board's last-read and success-rate text.
    /// </summary>
    private const float MeterInfoFontSize = 26f;

    /// <summary>
    /// Corner radius, in pixels, of the SmarterMeter board's usage/cost grid.
    /// </summary>
    private const float MeterGridCornerRadiusPx = 10f;

    /// <summary>
    /// Width, in pixels, of the SmarterMeter board's grid label column ("kWh"/"£").
    /// </summary>
    private const float MeterGridLabelColumnWidthPx = 90f;

    /// <summary>
    /// Height, in pixels, of the SmarterMeter board's grid header row ("Today"/"7d"/"30d").
    /// </summary>
    private const float MeterGridHeaderRowHeightPx = 58f;

    /// <summary>
    /// Font size, in points, used for the SmarterMeter board's grid headers and row labels.
    /// </summary>
    private const float MeterGridHeaderFontSize = 20f;

    /// <summary>
    /// Font size, in points, used for the SmarterMeter board's grid values.
    /// </summary>
    private const float MeterGridValueFontSize = 40f;

    /// <summary>
    /// Stroke thickness, in pixels, of the SmarterMeter board's box and grid borders.
    /// </summary>
    private const float MeterGridBorderThicknessPx = 2f;

    /// <summary>
    /// Height, in pixels, of each row in the RSS feed list.
    /// </summary>
    private const float RssRowHeightPx = 32f;

    /// <summary>
    /// Width, in pixels, of the RSS feed list's time column.
    /// </summary>
    private const float RssTimeColumnWidthPx = 80f;

    /// <summary>
    /// Width, in pixels, of the RSS feed list's title column.
    /// </summary>
    private const float RssTitleColumnWidthPx = 150f;

    /// <summary>
    /// Gap, in pixels, between the RSS feed list's columns.
    /// </summary>
    private const float RssColumnGapPx = 24f;

    /// <summary>
    /// The lookup table used for PNG chunk CRC32 checksums.
    /// </summary>
    private static readonly uint[] CrcTable = BuildCrcTable();

    /// <summary>
    /// Renders the current time and date to an 800x480 PNG, for display A - used as a fallback when Home Assistant data can't be fetched.
    /// </summary>
    /// <returns>The rendered image, encoded as a minimal 8-bit grayscale PNG.</returns>
    public byte[] RenderClock()
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont timeFont = new(boldTypeface, TimeFontSize);
        using SKFont dateFont = new(mediumTypeface, DateFontSize);
        using SKPaint paint = new() { Color = SKColors.Black, IsAntialias = true };

        DateTime now = DateTime.Now;
        string timeText = now.ToString("HH:mm");
        string dateText = now.ToString("dddd d MMMM yyyy");

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(SKColors.White);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.DrawText(timeText, WidthPx / 2f, HeightPx / 2f, SKTextAlign.Center, timeFont, paint);
            canvas.DrawText(dateText, WidthPx / 2f, (HeightPx / 2f) + 70f, SKTextAlign.Center, dateFont, paint);
        }

        return EncodeGrayscalePng(bitmap);
    }

    /// <summary>
    /// Renders the home screen - today's date and time, current weather, today's calendar events, and overdue/due-today tasks - to an 800x480 PNG, for display A.
    /// </summary>
    /// <param name="now">The current date and time.</param>
    /// <param name="weather">The current weather, or null if it could not be fetched.</param>
    /// <param name="events">Today's calendar events, sorted by start time.</param>
    /// <param name="tasks">Overdue and due-today tasks, sorted by due date.</param>
    /// <returns>The rendered image, encoded as a truecolor PNG.</returns>
    public byte[] RenderHomeScreen(DateTime now, HomeAssistantWeather? weather, List<HomeAssistantEvent> events, List<HomeAssistantTask> tasks)
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont dayNumberFont = new(boldTypeface, DayNumberFontSize);
        using SKFont weekdayFont = new(boldTypeface, WeekdayFontSize);
        using SKFont monthFont = new(mediumTypeface, WeekdayFontSize);
        using SKFont timeFont = new(boldTypeface, HomeTimeFontSize);
        using SKFont weatherTempFont = new(boldTypeface, WeatherTempFontSize);
        using SKFont weatherLabelFont = new(mediumTypeface, WeatherLabelFontSize);
        using SKFont sectionHeaderFont = new(boldTypeface, SectionHeaderFontSize);
        using SKFont eventTimeFont = new(boldTypeface, EventFontSize);
        using SKFont eventTitleFont = new(mediumTypeface, EventFontSize);
        using SKFont taskLabelFont = new(boldTypeface, TaskLabelFontSize);
        using SKFont taskTitleFont = new(mediumTypeface, EventFontSize);
        using SKPaint blackFill = new() { Color = SKColors.Black, IsAntialias = true };
        using SKPaint redFill = new() { Color = SKColors.Red, IsAntialias = true };
        using SKPaint greenFill = new() { Color = SKColors.Lime, IsAntialias = true };
        using SKPaint yellowFill = new() { Color = SKColors.Yellow, IsAntialias = true };
        using SKPaint blueFill = new() { Color = SKColors.Blue, IsAntialias = true };
        using SKPaint whiteFill = new() { Color = SKColors.White, IsAntialias = true };

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(SKColors.White);
        using SKCanvas canvas = new(bitmap);

        string dayText = now.Day.ToString();
        canvas.DrawText(dayText, HomeMarginPx, DayNumberBaselineY, SKTextAlign.Left, dayNumberFont, blackFill);
        float dateLabelX = HomeMarginPx + dayNumberFont.MeasureText(dayText) + HeaderDateGapPx;
        canvas.DrawText(now.ToString("dddd"), dateLabelX, WeekdayBaselineY, SKTextAlign.Left, weekdayFont, blackFill);
        canvas.DrawText(now.ToString("MMMM"), dateLabelX, MonthBaselineY, SKTextAlign.Left, monthFont, blackFill);

        float timeRight = WidthPx - HomeMarginPx;
        canvas.DrawText(now.ToString("HH:mm"), timeRight, HomeTimeBaselineY, SKTextAlign.Right, timeFont, blackFill);
        if (weather is not null)
        {
            string tempText = $"{Math.Round(weather.TemperatureC)}°C";
            float tempWidth = weatherTempFont.MeasureText(tempText);
            float labelWidth = weatherLabelFont.MeasureText(weather.Label);
            float labelX = timeRight - labelWidth;
            float tempX = labelX - WeatherRowGapPx - tempWidth;
            float iconCenterX = tempX - WeatherRowGapPx - WeatherIconRadiusPx;
            float iconCenterY = WeatherRowBaselineY - (WeatherIconRadiusPx * 0.5f);
            DrawWeatherIcon(canvas, weather.IconKind, iconCenterX, iconCenterY, WeatherIconRadiusPx, blackFill);
            canvas.DrawText(tempText, tempX, WeatherRowBaselineY, SKTextAlign.Left, weatherTempFont, blackFill);
            canvas.DrawText(weather.Label, labelX, WeatherRowBaselineY, SKTextAlign.Left, weatherLabelFont, blackFill);
        }

        canvas.DrawRect(new SKRect(HomeMarginPx, HeaderDividerY, WidthPx - HomeMarginPx, HeaderDividerY + DividerThicknessPx), blackFill);

        float columnTop = HeaderDividerY + DividerThicknessPx + BodyTopPaddingPx;
        float bodyLimitY = HeightPx - BodyBottomPaddingPx;
        float bodyWidth = WidthPx - (2f * HomeMarginPx);
        float allDayColumnX = HomeMarginPx;
        float dividerOneX = HomeMarginPx + (bodyWidth * AllDayColumnFraction);
        float eventsColumnX = dividerOneX + ColumnGapPx;
        float dividerTwoX = HomeMarginPx + (bodyWidth * (AllDayColumnFraction + EventsColumnFraction));
        float tasksColumnX = dividerTwoX + ColumnGapPx;
        float tasksColumnRight = WidthPx - HomeMarginPx;
        List<HomeAssistantEvent> allDayEvents = [.. events.Where(calendarEvent => calendarEvent.IsAllDay)];
        List<HomeAssistantEvent> timedEvents = [.. events.Where(calendarEvent => !calendarEvent.IsAllDay)];

        canvas.DrawText("ALL DAY", allDayColumnX, columnTop, SKTextAlign.Left, sectionHeaderFont, blackFill);
        float allDayColumnMaxWidth = dividerOneX - ColumnTextTrailingPaddingPx - allDayColumnX;
        float allDayY = columnTop + SectionHeaderGapPx;
        foreach (HomeAssistantEvent calendarEvent in allDayEvents)
        {
            if (allDayY > bodyLimitY)
                break;
            SKPaint eventFill = GetColorFill(calendarEvent.Color, blackFill, redFill, greenFill, yellowFill, blueFill);
            foreach (string line in WrapToLines(RemoveUnsupportedCharacters(calendarEvent.Title, eventTitleFont), eventTitleFont, allDayColumnMaxWidth, 2))
            {
                canvas.DrawText(line, allDayColumnX, allDayY, SKTextAlign.Left, eventTitleFont, eventFill);
                allDayY += WrapLineHeightPx;
            }
            allDayY += ItemGapPx;
        }

        canvas.DrawRect(new SKRect(dividerOneX - (DividerThicknessPx / 2f), columnTop - BodyTopPaddingPx, dividerOneX + (DividerThicknessPx / 2f), bodyLimitY), blackFill);

        canvas.DrawText("EVENTS", eventsColumnX, columnTop, SKTextAlign.Left, sectionHeaderFont, blackFill);
        float eventTitleX = eventsColumnX + EventTimeColumnWidthPx;
        float eventTitleMaxWidth = dividerTwoX - ColumnTextTrailingPaddingPx - eventTitleX;
        int elapsedTimedEventCount = timedEvents.Count(calendarEvent => calendarEvent.End <= now);
        int visibleTimedEventCapacity = CountItemsThatFit(timedEvents, eventTitleFont, eventTitleMaxWidth, columnTop + SectionHeaderGapPx, bodyLimitY);
        int timedEventStartIndex = elapsedTimedEventCount > 0 ? Math.Min(elapsedTimedEventCount, Math.Max(0, timedEvents.Count - visibleTimedEventCapacity)) : 0;
        List<HomeAssistantEvent> visibleTimedEvents = [.. timedEvents.Skip(timedEventStartIndex)];
        float eventY = columnTop + SectionHeaderGapPx;
        foreach (HomeAssistantEvent calendarEvent in visibleTimedEvents)
        {
            if (eventY > bodyLimitY)
                break;
            SKPaint eventFill = GetColorFill(calendarEvent.Color, blackFill, redFill, greenFill, yellowFill, blueFill);
            bool isPast = calendarEvent.End <= now;
            bool isCurrent = !isPast && calendarEvent.Start <= now;
            List<string> titleLines = WrapToLines(RemoveUnsupportedCharacters(calendarEvent.Title, eventTitleFont), eventTitleFont, eventTitleMaxWidth, 2);
            SKPaint textFill = isCurrent ? whiteFill : eventFill;
            if (isCurrent)
            {
                float highlightBottom = eventY + eventTitleFont.Metrics.Descent + EventHighlightPaddingPx + ((titleLines.Count - 1) * WrapLineHeightPx);
                SKRect highlightRect = new(eventsColumnX - EventHighlightHorizontalPaddingPx, eventY + eventTitleFont.Metrics.Ascent - EventHighlightPaddingPx, dividerTwoX - EventHighlightTrailingPaddingPx, highlightBottom);
                canvas.DrawRoundRect(highlightRect, EventHighlightCornerRadiusPx, EventHighlightCornerRadiusPx, eventFill);
            }
            canvas.DrawText(calendarEvent.Start.ToString("HH:mm"), eventsColumnX, eventY, SKTextAlign.Left, eventTimeFont, textFill);
            float lineY = eventY;
            for (int lineIndex = 0; lineIndex < titleLines.Count; lineIndex++)
            {
                canvas.DrawText(titleLines[lineIndex], eventTitleX, lineY, SKTextAlign.Left, eventTitleFont, textFill);
                if (isPast)
                {
                    float strikeStartX = lineIndex == 0 ? eventsColumnX : eventTitleX;
                    float strikeY = lineY + (eventTitleFont.Metrics.Ascent * 0.35f);
                    float strikeRight = eventTitleX + eventTitleFont.MeasureText(titleLines[lineIndex]);
                    using SKPaint strikePaint = new() { Color = eventFill.Color, IsAntialias = true, StrokeWidth = EventStrikeThicknessPx, Style = SKPaintStyle.Stroke };
                    canvas.DrawLine(strikeStartX, strikeY, strikeRight, strikeY, strikePaint);
                }
                lineY += WrapLineHeightPx;
            }
            eventY = lineY + ItemGapPx;
        }

        canvas.DrawRect(new SKRect(dividerTwoX - (DividerThicknessPx / 2f), columnTop - BodyTopPaddingPx, dividerTwoX + (DividerThicknessPx / 2f), bodyLimitY), blackFill);

        canvas.DrawText("TASKS", tasksColumnX, columnTop, SKTextAlign.Left, sectionHeaderFont, blackFill);
        float taskTitleMaxWidth = tasksColumnRight - tasksColumnX;
        float taskY = columnTop + SectionHeaderGapPx;
        foreach (HomeAssistantTask task in tasks)
        {
            if (taskY > bodyLimitY)
                break;
            bool isOverdue = task.Status == HomeAssistantTaskStatus.Overdue;
            List<string> titleLines = WrapToLines(RemoveUnsupportedCharacters(task.Title, taskTitleFont), taskTitleFont, taskTitleMaxWidth, 2);
            if (isOverdue)
            {
                canvas.DrawText("OVERDUE", tasksColumnX, taskY, SKTextAlign.Left, taskLabelFont, redFill);
                taskY += TaskOverdueLabelGapPx;
            }
            foreach (string line in titleLines)
            {
                canvas.DrawText(line, tasksColumnX, taskY, SKTextAlign.Left, taskTitleFont, blackFill);
                taskY += WrapLineHeightPx;
            }
            taskY += ItemGapPx;
        }

        return EncodeRgbPng(bitmap);
    }

    /// <summary>
    /// Renders the Kuma status board - an overall status banner followed by each host's uptime, monitor count, and recent check history - to an 800x480 PNG, for display B.
    /// </summary>
    /// <param name="summary">The status summary fetched from Kuma, or null if it could not be fetched.</param>
    /// <returns>The rendered image, encoded as a truecolor PNG.</returns>
    public byte[] RenderKumaStatus(KumaStatusSummary? summary)
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont statusFont = new(boldTypeface, KumaStatusLineFontSize);
        using SKFont columnHeaderFont = new(boldTypeface, SectionHeaderFontSize);
        using SKFont hostNameFont = new(boldTypeface, EventFontSize);
        using SKFont rowValueFont = new(mediumTypeface, EventFontSize);
        using SKFont upCountFont = new(boldTypeface, EventFontSize);
        using SKPaint blackFill = new() { Color = SKColors.Black, IsAntialias = true };
        using SKPaint redFill = new() { Color = SKColors.Red, IsAntialias = true };
        using SKPaint greenFill = new() { Color = SKColors.Lime, IsAntialias = true };
        using SKPaint yellowFill = new() { Color = SKColors.Yellow, IsAntialias = true };
        using SKPaint blueFill = new() { Color = SKColors.Blue, IsAntialias = true };
        using SKPaint whiteFill = new() { Color = SKColors.White, IsAntialias = true };

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(SKColors.White);
        using SKCanvas canvas = new(bitmap);

        if (summary is null)
        {
            canvas.DrawText("Unable to reach Kuma", HomeMarginPx, KumaStatusLineBaselineY, SKTextAlign.Left, statusFont, blackFill);
            return EncodeRgbPng(bitmap);
        }

        (string statusText, SKPaint statusFill) = GetStatusDisplay(summary.OverallStatus, blackFill, redFill, greenFill, yellowFill, blueFill);
        float iconCenterX = HomeMarginPx + KumaStatusIconRadiusPx;
        float iconCenterY = KumaStatusLineBaselineY - (KumaStatusLineFontSize * 0.32f);
        DrawStatusIcon(canvas, summary.OverallStatus, iconCenterX, iconCenterY, KumaStatusIconRadiusPx, statusFill, whiteFill);
        canvas.DrawText(statusText, iconCenterX + KumaStatusIconRadiusPx + KumaStatusIconTextGapPx, KumaStatusLineBaselineY, SKTextAlign.Left, statusFont, statusFill);

        float hostColumnX = HomeMarginPx;
        float uptimeColumnX = hostColumnX + KumaHostColumnWidthPx;
        float upColumnX = uptimeColumnX + KumaUptimeColumnWidthPx;
        float totalColumnX = upColumnX + KumaUpColumnWidthPx;
        float dividerX = totalColumnX + KumaTotalColumnWidthPx;
        float historyColumnX = dividerX + ColumnGapPx;
        float historyColumnRight = WidthPx - HomeMarginPx;
        float columnTop = KumaDividerY + DividerThicknessPx + BodyTopPaddingPx;
        canvas.DrawText("HOST", hostColumnX, columnTop, SKTextAlign.Left, columnHeaderFont, blackFill);
        canvas.DrawText("UPTIME", uptimeColumnX, columnTop, SKTextAlign.Left, columnHeaderFont, blackFill);
        canvas.DrawText("UP", upColumnX, columnTop, SKTextAlign.Left, columnHeaderFont, blackFill);
        canvas.DrawText("TOTAL", totalColumnX, columnTop, SKTextAlign.Left, columnHeaderFont, blackFill);
        canvas.DrawText("HISTORY", historyColumnX, columnTop, SKTextAlign.Left, columnHeaderFont, blackFill);

        float bodyLimitY = HeightPx - BodyBottomPaddingPx;
        int hostCount = Math.Max(summary.Hosts.Count, 1);
        float rowHeight = (bodyLimitY - columnTop) / hostCount;
        float historyColumnWidth = historyColumnRight - historyColumnX;
        int maxTickCount = Math.Max((int)(historyColumnWidth / KumaTickSpacingPx), 1);

        for (int rowIndex = 0; rowIndex < summary.Hosts.Count; rowIndex++)
        {
            KumaHostStatus host = summary.Hosts[rowIndex];
            float rowCenter = columnTop + (rowIndex * rowHeight) + (rowHeight / 2f);
            float textBaseline = rowCenter - ((rowValueFont.Metrics.Ascent + rowValueFont.Metrics.Descent) / 2f);
            canvas.DrawText(host.Name, hostColumnX, textBaseline, SKTextAlign.Left, hostNameFont, blackFill);
            canvas.DrawText($"{host.UptimeRatio:P2}", uptimeColumnX, textBaseline, SKTextAlign.Left, rowValueFont, blackFill);
            SKPaint upCountFill = host.UpCount == host.TotalCount ? greenFill : redFill;
            canvas.DrawText(host.UpCount.ToString(), upColumnX, textBaseline, SKTextAlign.Left, upCountFont, upCountFill);
            canvas.DrawText(host.TotalCount.ToString(), totalColumnX, textBaseline, SKTextAlign.Left, rowValueFont, blackFill);

            List<bool> displayedChecks = [.. host.RecentCheckGroups.TakeLast(maxTickCount)];
            float tickSpacing = displayedChecks.Count > 1 ? historyColumnWidth / (displayedChecks.Count - 1) : 0f;
            float tickTop = rowCenter - (KumaTickHeightPx / 2f);
            float tickBottom = rowCenter + (KumaTickHeightPx / 2f);
            using SKPaint tickPaint = new() { IsAntialias = true, StrokeWidth = KumaTickThicknessPx, StrokeCap = SKStrokeCap.Round };
            for (int tickIndex = 0; tickIndex < displayedChecks.Count; tickIndex++)
            {
                float tickX = historyColumnX + (tickIndex * tickSpacing);
                tickPaint.Color = displayedChecks[tickIndex] ? SKColors.Lime : SKColors.Red;
                canvas.DrawLine(tickX, tickTop, tickX, tickBottom, tickPaint);
            }
        }

        return EncodeRgbPng(bitmap);
    }

    /// <summary>
    /// Renders the SmarterMeter status board - the current reading, last read time, and capture success rate, followed by usage and cost for today, the last 7 days, and the last 30 days - to an 800x480 PNG, for display C. Matches the layout of the HASmarterMeterCard Lovelace card.
    /// </summary>
    /// <param name="summary">The usage and cost summary, or null if it could not be calculated.</param>
    /// <returns>The rendered image, encoded as a truecolor PNG.</returns>
    public byte[] RenderMeterSummary(MeterSummary? summary)
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont readingFont = new(boldTypeface, MeterReadingFontSize);
        using SKFont readingUnitFont = new(mediumTypeface, MeterReadingUnitFontSize);
        using SKFont infoFont = new(boldTypeface, MeterInfoFontSize);
        using SKFont gridHeaderFont = new(boldTypeface, MeterGridHeaderFontSize);
        using SKFont gridValueFont = new(boldTypeface, MeterGridValueFontSize);
        using SKPaint blackFill = new() { Color = SKColors.Black, IsAntialias = true };
        using SKPaint borderPaint = new() { Color = SKColors.Black, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = MeterGridBorderThicknessPx };

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(SKColors.White);
        using SKCanvas canvas = new(bitmap);

        if (summary is null)
        {
            canvas.DrawText("Unable to reach SmarterMeter", HomeMarginPx, HeightPx / 2f, SKTextAlign.Left, infoFont, blackFill);
            return EncodeRgbPng(bitmap);
        }

        float bodyWidth = WidthPx - (2f * HomeMarginPx);
        float topRowTop = HomeMarginPx;
        float topRowBottom = topRowTop + MeterTopBoxHeightPx;
        float availableTopRowWidth = bodyWidth - (2f * MeterTopBoxGapPx);
        float topBoxTotalUnits = MeterReadingBoxWidthUnits + MeterLastReadBoxWidthUnits + MeterSuccessBoxWidthUnits;
        float topBoxUnitWidth = availableTopRowWidth / topBoxTotalUnits;
        float readingBoxWidth = topBoxUnitWidth * MeterReadingBoxWidthUnits;
        float lastReadBoxWidth = topBoxUnitWidth * MeterLastReadBoxWidthUnits;
        float successBoxWidth = topBoxUnitWidth * MeterSuccessBoxWidthUnits;
        float readingBoxX = HomeMarginPx;
        float lastReadBoxX = readingBoxX + readingBoxWidth + MeterTopBoxGapPx;
        float successBoxX = lastReadBoxX + lastReadBoxWidth + MeterTopBoxGapPx;
        SKRect readingBox = new(readingBoxX, topRowTop, readingBoxX + readingBoxWidth, topRowBottom);
        SKRect lastReadBox = new(lastReadBoxX, topRowTop, lastReadBoxX + lastReadBoxWidth, topRowBottom);
        SKRect successBox = new(successBoxX, topRowTop, successBoxX + successBoxWidth, topRowBottom);
        canvas.DrawRoundRect(readingBox, MeterTopBoxCornerRadiusPx, MeterTopBoxCornerRadiusPx, borderPaint);
        canvas.DrawRoundRect(lastReadBox, MeterTopBoxCornerRadiusPx, MeterTopBoxCornerRadiusPx, borderPaint);
        canvas.DrawRoundRect(successBox, MeterTopBoxCornerRadiusPx, MeterTopBoxCornerRadiusPx, borderPaint);

        string readingText = summary.CurrentReading.ToString("N0");
        float readingTextWidth = readingFont.MeasureText(readingText);
        float readingUnitWidth = readingUnitFont.MeasureText(" kWh");
        float readingBaseline = readingBox.MidY - ((readingFont.Metrics.Ascent + readingFont.Metrics.Descent) / 2f);
        float readingStartX = readingBox.MidX - ((readingTextWidth + readingUnitWidth) / 2f);
        canvas.DrawText(readingText, readingStartX, readingBaseline, SKTextAlign.Left, readingFont, blackFill);
        canvas.DrawText(" kWh", readingStartX + readingTextWidth, readingBaseline, SKTextAlign.Left, readingUnitFont, blackFill);

        string lastReadText = $"Last Read: {summary.LastCapturedAt.ToLocalTime():HH:mm}";
        float infoBaseline = lastReadBox.MidY - ((infoFont.Metrics.Ascent + infoFont.Metrics.Descent) / 2f);
        canvas.DrawText(lastReadText, lastReadBox.MidX, infoBaseline, SKTextAlign.Center, infoFont, blackFill);

        string successText = $"{summary.SuccessRate.ToString("0.#")}%";
        canvas.DrawText(successText, successBox.MidX, infoBaseline, SKTextAlign.Center, infoFont, blackFill);

        float gridTop = topRowBottom + BodyTopPaddingPx;
        float gridBottom = HeightPx - BodyBottomPaddingPx;
        float gridLeft = HomeMarginPx;
        float gridRight = WidthPx - HomeMarginPx;
        float labelColumnRight = gridLeft + MeterGridLabelColumnWidthPx;
        float dataColumnWidth = (gridRight - labelColumnRight) / 3f;
        float todayColumnX = labelColumnRight;
        float weekColumnX = todayColumnX + dataColumnWidth;
        float monthColumnX = weekColumnX + dataColumnWidth;
        float headerRowBottom = gridTop + MeterGridHeaderRowHeightPx;
        float dataRowHeight = (gridBottom - headerRowBottom) / 2f;
        float usageRowBottom = headerRowBottom + dataRowHeight;

        canvas.DrawRoundRect(new SKRect(gridLeft, gridTop, gridRight, gridBottom), MeterGridCornerRadiusPx, MeterGridCornerRadiusPx, borderPaint);
        canvas.DrawLine(labelColumnRight, gridTop, labelColumnRight, gridBottom, borderPaint);
        canvas.DrawLine(weekColumnX, gridTop, weekColumnX, gridBottom, borderPaint);
        canvas.DrawLine(monthColumnX, gridTop, monthColumnX, gridBottom, borderPaint);
        canvas.DrawLine(gridLeft, headerRowBottom, gridRight, headerRowBottom, borderPaint);
        canvas.DrawLine(gridLeft, usageRowBottom, gridRight, usageRowBottom, borderPaint);

        float headerBaseline = gridTop + ((headerRowBottom - gridTop) / 2f) - ((gridHeaderFont.Metrics.Ascent + gridHeaderFont.Metrics.Descent) / 2f);
        canvas.DrawText("Today", todayColumnX + (dataColumnWidth / 2f), headerBaseline, SKTextAlign.Center, gridHeaderFont, blackFill);
        canvas.DrawText("7d", weekColumnX + (dataColumnWidth / 2f), headerBaseline, SKTextAlign.Center, gridHeaderFont, blackFill);
        canvas.DrawText("30d", monthColumnX + (dataColumnWidth / 2f), headerBaseline, SKTextAlign.Center, gridHeaderFont, blackFill);

        float usageRowCenter = headerRowBottom + (dataRowHeight / 2f);
        float usageLabelBaseline = usageRowCenter - ((gridHeaderFont.Metrics.Ascent + gridHeaderFont.Metrics.Descent) / 2f);
        float usageValueBaseline = usageRowCenter - ((gridValueFont.Metrics.Ascent + gridValueFont.Metrics.Descent) / 2f);
        canvas.DrawText("kWh", gridLeft + (MeterGridLabelColumnWidthPx / 2f), usageLabelBaseline, SKTextAlign.Center, gridHeaderFont, blackFill);
        canvas.DrawText(summary.TodayUsage.ToString("N0"), todayColumnX + (dataColumnWidth / 2f), usageValueBaseline, SKTextAlign.Center, gridValueFont, blackFill);
        canvas.DrawText(summary.WeekUsage.ToString("N0"), weekColumnX + (dataColumnWidth / 2f), usageValueBaseline, SKTextAlign.Center, gridValueFont, blackFill);
        canvas.DrawText(summary.MonthUsage.ToString("N0"), monthColumnX + (dataColumnWidth / 2f), usageValueBaseline, SKTextAlign.Center, gridValueFont, blackFill);

        float costRowCenter = usageRowBottom + (dataRowHeight / 2f);
        float costLabelBaseline = costRowCenter - ((gridHeaderFont.Metrics.Ascent + gridHeaderFont.Metrics.Descent) / 2f);
        float costValueBaseline = costRowCenter - ((gridValueFont.Metrics.Ascent + gridValueFont.Metrics.Descent) / 2f);
        canvas.DrawText("£", gridLeft + (MeterGridLabelColumnWidthPx / 2f), costLabelBaseline, SKTextAlign.Center, gridHeaderFont, blackFill);
        canvas.DrawText($"£{summary.TodayCost:0.00}", todayColumnX + (dataColumnWidth / 2f), costValueBaseline, SKTextAlign.Center, gridValueFont, blackFill);
        canvas.DrawText($"£{summary.WeekCost:0.00}", weekColumnX + (dataColumnWidth / 2f), costValueBaseline, SKTextAlign.Center, gridValueFont, blackFill);
        canvas.DrawText($"£{summary.MonthCost:0.00}", monthColumnX + (dataColumnWidth / 2f), costValueBaseline, SKTextAlign.Center, gridValueFont, blackFill);

        return EncodeRgbPng(bitmap);
    }

    /// <summary>
    /// Renders the RSS feed as a plain list of rows - time, title, and content, as many as fit - for display D. Matches the font used for the home screen's event list.
    /// </summary>
    /// <param name="items">The feed's items, newest first, or null if the feed could not be fetched.</param>
    /// <returns>The rendered image, encoded as a truecolor PNG.</returns>
    public byte[] RenderRssFeed(List<RssFeedItem>? items)
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont timeFont = new(boldTypeface, EventFontSize);
        using SKFont textFont = new(mediumTypeface, EventFontSize);
        using SKPaint blackFill = new() { Color = SKColors.Black, IsAntialias = true };

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(SKColors.White);
        using SKCanvas canvas = new(bitmap);

        if (items is null)
        {
            canvas.DrawText("Unable to reach the RSS feed", HomeMarginPx, HeightPx / 2f, SKTextAlign.Left, textFont, blackFill);
            return EncodeRgbPng(bitmap);
        }

        float bodyTop = HomeMarginPx;
        float bodyBottom = HeightPx - HomeMarginPx;
        int maxRowCount = (int)((bodyBottom - bodyTop) / RssRowHeightPx);
        float titleColumnX = HomeMarginPx + RssTimeColumnWidthPx + RssColumnGapPx;
        float contentColumnX = titleColumnX + RssTitleColumnWidthPx + RssColumnGapPx;
        float contentMaxWidth = (WidthPx - HomeMarginPx) - contentColumnX;

        for (int rowIndex = 0; rowIndex < Math.Min(items.Count, maxRowCount); rowIndex++)
        {
            RssFeedItem item = items[rowIndex];
            float rowCenter = bodyTop + (rowIndex * RssRowHeightPx) + (RssRowHeightPx / 2f);
            float baseline = rowCenter - ((textFont.Metrics.Ascent + textFont.Metrics.Descent) / 2f);
            string title = RemoveUnsupportedCharacters(item.Title, textFont);
            string content = TruncateToWidth(RemoveUnsupportedCharacters(item.Content, textFont), textFont, contentMaxWidth);
            canvas.DrawText(item.PublishedAt.ToLocalTime().ToString("HH:mm"), HomeMarginPx, baseline, SKTextAlign.Left, timeFont, blackFill);
            canvas.DrawText(title, titleColumnX, baseline, SKTextAlign.Left, textFont, blackFill);
            canvas.DrawText(content, contentColumnX, baseline, SKTextAlign.Left, textFont, blackFill);
        }

        return EncodeRgbPng(bitmap);
    }

    /// <summary>
    /// Renders a placeholder display, for a display letter without dedicated content yet - the letter and a small caption, in white, over the letter's assigned colour.
    /// </summary>
    /// <param name="letter">The display letter, e.g. "C".</param>
    /// <param name="colour">The display's assigned background colour.</param>
    /// <returns>The rendered image, encoded as a truecolor PNG.</returns>
    public byte[] RenderPlaceholder(string letter, SKColor colour)
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont letterFont = new(boldTypeface, PlaceholderLetterFontSize);
        using SKFont captionFont = new(mediumTypeface, PlaceholderCaptionFontSize);
        using SKPaint paint = new() { Color = SKColors.White, IsAntialias = true };

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(colour);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.DrawText(letter, WidthPx / 2f, HeightPx / 2f, SKTextAlign.Center, letterFont, paint);
            canvas.DrawText($"Display {letter}", WidthPx / 2f, (HeightPx / 2f) + 110f, SKTextAlign.Center, captionFont, paint);
        }

        return EncodeRgbPng(bitmap);
    }

    /// <summary>
    /// Picks the paint matching a calendar's configured colour name, falling back to black for an unrecognised name.
    /// </summary>
    /// <param name="colorName">The colour name, e.g. "Red" or "Green".</param>
    /// <param name="blackFill">The black paint.</param>
    /// <param name="redFill">The red paint.</param>
    /// <param name="greenFill">The green paint.</param>
    /// <param name="yellowFill">The yellow paint.</param>
    /// <param name="blueFill">The blue paint.</param>
    private static SKPaint GetColorFill(string colorName, SKPaint blackFill, SKPaint redFill, SKPaint greenFill, SKPaint yellowFill, SKPaint blueFill) => colorName switch
    {
        "Red" => redFill,
        "Green" => greenFill,
        "Yellow" => yellowFill,
        "Blue" => blueFill,
        _ => blackFill
    };

    /// <summary>
    /// Picks the status line's text and paint for the given overall status, matching the states Kuma's own status page shows.
    /// </summary>
    /// <param name="status">The overall status to display.</param>
    /// <param name="blackFill">The black paint, used as a fallback for an unrecognised status.</param>
    /// <param name="redFill">The red paint.</param>
    /// <param name="greenFill">The green paint.</param>
    /// <param name="yellowFill">The yellow paint.</param>
    /// <param name="blueFill">The blue paint.</param>
    private static (string Text, SKPaint Fill) GetStatusDisplay(KumaOverallStatus status, SKPaint blackFill, SKPaint redFill, SKPaint greenFill, SKPaint yellowFill, SKPaint blueFill) => status switch
    {
        KumaOverallStatus.AllUp => ("All Systems Operational", greenFill),
        KumaOverallStatus.PartialDown => ("Partially Degraded Service", yellowFill),
        KumaOverallStatus.AllDown => ("Degraded Service", redFill),
        KumaOverallStatus.Maintenance => ("Under Maintenance", blueFill),
        _ => ("Status Unavailable", blackFill)
    };

    /// <summary>
    /// Shortens the given text with a trailing ellipsis if it's wider than the given maximum width.
    /// </summary>
    /// <param name="text">The text to measure and shorten.</param>
    /// <param name="font">The font the text will be drawn with.</param>
    /// <param name="maxWidth">The maximum width, in pixels, the text may occupy.</param>
    private static string TruncateToWidth(string text, SKFont font, float maxWidth)
    {
        if (font.MeasureText(text) <= maxWidth)
            return text;
        const string ellipsis = "…";
        int lo = 0;
        int hi = text.Length;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            string candidate = text[..mid].TrimEnd() + ellipsis;
            if (font.MeasureText(candidate) <= maxWidth)
                lo = mid + 1;
            else
                hi = mid;
        }
        return text[..Math.Max(lo - 1, 0)].TrimEnd() + ellipsis;
    }

    /// <summary>
    /// Removes any characters the given font can't render, such as emoji, so text pulled from external sources - calendar events, tasks, RSS items - never produces missing-glyph boxes on the e-ink display.
    /// </summary>
    /// <param name="text">The text to sanitize.</param>
    /// <param name="font">The font the text will be drawn with.</param>
    private static string RemoveUnsupportedCharacters(string text, SKFont font)
    {
        StringBuilder builder = new(text.Length);
        foreach (Rune rune in text.EnumerateRunes())
            if (font.ContainsGlyph(rune.Value))
                builder.Append(rune.ToString());
        return builder.ToString();
    }

    /// <summary>
    /// Counts how many items, starting from the first, fit within the given vertical space when each is wrapped to at most two lines.
    /// </summary>
    /// <param name="events">The events to measure, in display order.</param>
    /// <param name="font">The font each title will be drawn with.</param>
    /// <param name="maxWidth">The maximum width, in pixels, each line may occupy.</param>
    /// <param name="startY">The y-position the first item would be drawn at.</param>
    /// <param name="limitY">The y-position beyond which no more items may be drawn.</param>
    private static int CountItemsThatFit(List<HomeAssistantEvent> events, SKFont font, float maxWidth, float startY, float limitY)
    {
        float y = startY;
        int count = 0;
        foreach (HomeAssistantEvent calendarEvent in events)
        {
            if (y > limitY)
                break;
            int lineCount = WrapToLines(RemoveUnsupportedCharacters(calendarEvent.Title, font), font, maxWidth, 2).Count;
            y += (lineCount * WrapLineHeightPx) + ItemGapPx;
            count++;
        }
        return count;
    }

    /// <summary>
    /// Wraps text to at most the given number of lines, word-wrapping within the given width and ellipsising the final line if the text doesn't fit.
    /// </summary>
    /// <param name="text">The text to wrap.</param>
    /// <param name="font">The font the text will be drawn with.</param>
    /// <param name="maxWidth">The maximum width, in pixels, each line may occupy.</param>
    /// <param name="maxLines">The maximum number of lines to produce.</param>
    private static List<string> WrapToLines(string text, SKFont font, float maxWidth, int maxLines)
    {
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        List<string> lines = [];
        int wordIndex = 0;
        while (wordIndex < words.Length && lines.Count < maxLines - 1)
        {
            string line = words[wordIndex];
            wordIndex++;
            while (wordIndex < words.Length && font.MeasureText($"{line} {words[wordIndex]}") <= maxWidth)
            {
                line = $"{line} {words[wordIndex]}";
                wordIndex++;
            }
            lines.Add(line);
        }
        if (wordIndex < words.Length)
            lines.Add(TruncateToWidth(string.Join(' ', words[wordIndex..]), font, maxWidth));
        return lines;
    }

    /// <summary>
    /// Draws the small weather icon for the given condition, centred at the given point.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="iconKind">The icon shape to draw.</param>
    /// <param name="centerX">The icon's horizontal centre.</param>
    /// <param name="centerY">The icon's vertical centre.</param>
    /// <param name="radius">The icon's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    private static void DrawWeatherIcon(SKCanvas canvas, WeatherIconKind iconKind, float centerX, float centerY, float radius, SKPaint fill)
    {
        switch (iconKind)
        {
            case WeatherIconKind.Sunny:
                DrawSun(canvas, centerX, centerY, radius, fill, true);
                break;
            case WeatherIconKind.ClearNight:
                DrawSun(canvas, centerX, centerY, radius, fill, false);
                break;
            case WeatherIconKind.PartlyCloudy:
                DrawSun(canvas, centerX + (radius * 0.35f), centerY - (radius * 0.35f), radius * 0.55f, fill, true);
                DrawCloud(canvas, centerX - (radius * 0.1f), centerY + (radius * 0.2f), radius * 0.85f, fill);
                break;
            case WeatherIconKind.Rain:
                DrawCloud(canvas, centerX, centerY - (radius * 0.2f), radius * 0.85f, fill);
                DrawRainDrops(canvas, centerX, centerY, radius, fill);
                break;
            case WeatherIconKind.Snow:
                DrawCloud(canvas, centerX, centerY - (radius * 0.2f), radius * 0.85f, fill);
                DrawSnowDots(canvas, centerX, centerY, radius, fill);
                break;
            case WeatherIconKind.Thunder:
                DrawCloud(canvas, centerX, centerY - (radius * 0.2f), radius * 0.85f, fill);
                DrawBolt(canvas, centerX, centerY, radius, fill);
                break;
            case WeatherIconKind.Windy:
                DrawWindLines(canvas, centerX, centerY, radius, fill);
                break;
            case WeatherIconKind.Cloudy:
            default:
                DrawCloud(canvas, centerX, centerY, radius, fill);
                break;
        }
    }

    /// <summary>
    /// Draws the status line's icon - a filled circle in the status's colour with a simple check, exclamation, cross, or wrench mark - centred at the given point.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="status">The overall status to draw the icon for.</param>
    /// <param name="centerX">The icon's horizontal centre.</param>
    /// <param name="centerY">The icon's vertical centre.</param>
    /// <param name="radius">The icon's overall radius.</param>
    /// <param name="fill">The paint to fill the circle with.</param>
    /// <param name="markFill">The paint to draw the mark with.</param>
    private static void DrawStatusIcon(SKCanvas canvas, KumaOverallStatus status, float centerX, float centerY, float radius, SKPaint fill, SKPaint markFill)
    {
        canvas.DrawCircle(centerX, centerY, radius, fill);
        using SKPaint markPaint = new() { Color = markFill.Color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = radius * 0.28f, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
        switch (status)
        {
            case KumaOverallStatus.AllUp:
                canvas.DrawLine(centerX - (radius * 0.45f), centerY, centerX - (radius * 0.1f), centerY + (radius * 0.35f), markPaint);
                canvas.DrawLine(centerX - (radius * 0.1f), centerY + (radius * 0.35f), centerX + (radius * 0.5f), centerY - (radius * 0.35f), markPaint);
                break;
            case KumaOverallStatus.PartialDown:
                canvas.DrawLine(centerX, centerY - (radius * 0.5f), centerX, centerY + (radius * 0.1f), markPaint);
                canvas.DrawCircle(centerX, centerY + (radius * 0.45f), radius * 0.09f, markFill);
                break;
            case KumaOverallStatus.AllDown:
                canvas.DrawLine(centerX - (radius * 0.4f), centerY - (radius * 0.4f), centerX + (radius * 0.4f), centerY + (radius * 0.4f), markPaint);
                canvas.DrawLine(centerX - (radius * 0.4f), centerY + (radius * 0.4f), centerX + (radius * 0.4f), centerY - (radius * 0.4f), markPaint);
                break;
            case KumaOverallStatus.Maintenance:
                canvas.DrawLine(centerX - (radius * 0.45f), centerY, centerX + (radius * 0.45f), centerY, markPaint);
                break;
        }
    }

    /// <summary>
    /// Draws a filled sun, with optional rays, centred at the given point.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="centerX">The sun's horizontal centre.</param>
    /// <param name="centerY">The sun's vertical centre.</param>
    /// <param name="radius">The sun's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    /// <param name="withRays">Whether to draw the sun's rays.</param>
    private static void DrawSun(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fill, bool withRays)
    {
        canvas.DrawCircle(centerX, centerY, radius * 0.6f, fill);
        if (!withRays)
            return;
        using SKPaint rayPaint = new() { Color = fill.Color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round };
        for (int i = 0; i < 8; i++)
        {
            double angle = i * Math.PI / 4;
            float x1 = centerX + ((float)Math.Cos(angle) * radius * 0.85f);
            float y1 = centerY + ((float)Math.Sin(angle) * radius * 0.85f);
            float x2 = centerX + ((float)Math.Cos(angle) * radius * 1.15f);
            float y2 = centerY + ((float)Math.Sin(angle) * radius * 1.15f);
            canvas.DrawLine(x1, y1, x2, y2, rayPaint);
        }
    }

    /// <summary>
    /// Draws a filled cloud shape, centred at the given point.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="centerX">The cloud's horizontal centre.</param>
    /// <param name="centerY">The cloud's vertical centre.</param>
    /// <param name="radius">The cloud's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    private static void DrawCloud(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fill)
    {
        canvas.DrawOval(new SKRect(centerX - (radius * 0.15f), centerY - (radius * 0.75f), centerX + (radius * 0.55f), centerY - (radius * 0.05f)), fill);
        canvas.DrawOval(new SKRect(centerX - (radius * 0.75f), centerY - (radius * 0.35f), centerX + (radius * 0.05f), centerY + (radius * 0.35f)), fill);
        canvas.DrawRoundRect(new SKRect(centerX - (radius * 0.95f), centerY - (radius * 0.05f), centerX + (radius * 0.75f), centerY + (radius * 0.5f)), radius * 0.28f, radius * 0.28f, fill);
    }

    /// <summary>
    /// Draws three short diagonal rain drops beneath a cloud.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="centerX">The icon's horizontal centre.</param>
    /// <param name="centerY">The icon's vertical centre.</param>
    /// <param name="radius">The icon's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    private static void DrawRainDrops(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fill)
    {
        using SKPaint dropPaint = new() { Color = fill.Color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 4f, StrokeCap = SKStrokeCap.Round };
        float[] offsets = [-0.3f, 0.05f, 0.4f];
        foreach (float offset in offsets)
        {
            float x = centerX + (offset * radius);
            canvas.DrawLine(x, centerY + (radius * 0.45f), x - 5f, centerY + (radius * 0.95f), dropPaint);
        }
    }

    /// <summary>
    /// Draws three small snow dots beneath a cloud.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="centerX">The icon's horizontal centre.</param>
    /// <param name="centerY">The icon's vertical centre.</param>
    /// <param name="radius">The icon's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    private static void DrawSnowDots(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fill)
    {
        float[] offsets = [-0.3f, 0.05f, 0.4f];
        foreach (float offset in offsets)
            canvas.DrawCircle(centerX + (offset * radius), centerY + (radius * 0.7f), radius * 0.09f, fill);
    }

    /// <summary>
    /// Draws a simple lightning bolt beneath a cloud.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="centerX">The icon's horizontal centre.</param>
    /// <param name="centerY">The icon's vertical centre.</param>
    /// <param name="radius">The icon's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    private static void DrawBolt(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fill)
    {
        SKPathBuilder pathBuilder = new();
        pathBuilder.MoveTo(centerX + (radius * 0.15f), centerY + (radius * 0.35f));
        pathBuilder.LineTo(centerX - (radius * 0.2f), centerY + (radius * 0.75f));
        pathBuilder.LineTo(centerX + (radius * 0.05f), centerY + (radius * 0.75f));
        pathBuilder.LineTo(centerX - (radius * 0.15f), centerY + (radius * 1.1f));
        pathBuilder.LineTo(centerX + (radius * 0.35f), centerY + (radius * 0.6f));
        pathBuilder.LineTo(centerX + (radius * 0.1f), centerY + (radius * 0.6f));
        pathBuilder.Close();
        using SKPath path = pathBuilder.Detach();
        canvas.DrawPath(path, fill);
    }

    /// <summary>
    /// Draws three horizontal wind lines of decreasing length.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="centerX">The icon's horizontal centre.</param>
    /// <param name="centerY">The icon's vertical centre.</param>
    /// <param name="radius">The icon's overall radius.</param>
    /// <param name="fill">The paint to draw with.</param>
    private static void DrawWindLines(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fill)
    {
        using SKPaint linePaint = new() { Color = fill.Color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 4f, StrokeCap = SKStrokeCap.Round };
        float[] lengths = [0.9f, 0.7f, 0.5f];
        float[] yOffsets = [-0.4f, 0f, 0.4f];
        for (int i = 0; i < lengths.Length; i++)
            canvas.DrawLine(centerX - (radius * lengths[i]), centerY + (radius * yOffsets[i]), centerX + (radius * 0.9f), centerY + (radius * yOffsets[i]), linePaint);
    }

    /// <summary>
    /// Loads a bundled typeface from disk.
    /// </summary>
    /// <param name="fontRelativePath">The font file's path, relative to the content root.</param>
    private SKTypeface LoadTypeface(string fontRelativePath)
    {
        string fontPath = Path.Combine(env.ContentRootPath, fontRelativePath);
        return SKTypeface.FromFile(fontPath) ?? throw new InvalidOperationException($"Could not load font at {fontPath}");
    }

    /// <summary>
    /// Encodes the given bitmap as a hand-built, minimal 8-bit grayscale PNG - just IHDR, one IDAT, and IEND, with no ancillary chunks - to sidestep constrained embedded decoders that don't tolerate chunks or colour types beyond the basics.
    /// </summary>
    /// <param name="bitmap">The bitmap to encode, assumed to contain only black/white/grey content (equal R, G, and B per pixel).</param>
    /// <returns>The encoded PNG bytes.</returns>
    private static byte[] EncodeGrayscalePng(SKBitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        byte[] raw = new byte[height * (1 + width)];
        int rawIndex = 0;
        for (int y = 0; y < height; y++)
        {
            raw[rawIndex++] = 0;
            for (int x = 0; x < width; x++)
                raw[rawIndex++] = bitmap.GetPixel(x, y).Red;
        }

        using MemoryStream compressedStream = new();
        using (ZLibStream zLibStream = new(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
            zLibStream.Write(raw, 0, raw.Length);
        byte[] compressed = compressedStream.ToArray();

        byte[] ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;
        ihdr[9] = 0;
        ihdr[10] = 0;
        ihdr[11] = 0;
        ihdr[12] = 0;

        using MemoryStream output = new();
        output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], 0, 8);
        WriteChunk(output, "IHDR", ihdr);
        WriteChunk(output, "IDAT", compressed);
        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    /// <summary>
    /// Encodes the given bitmap as a hand-built, minimal 8-bit truecolor (RGB, no alpha) PNG - just IHDR, one IDAT, and IEND, with no ancillary chunks - to sidestep constrained embedded decoders that don't tolerate chunks or colour types beyond the basics.
    /// </summary>
    /// <param name="bitmap">The bitmap to encode.</param>
    /// <returns>The encoded PNG bytes.</returns>
    private static byte[] EncodeRgbPng(SKBitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        byte[] raw = new byte[height * (1 + (width * 3))];
        int rawIndex = 0;
        for (int y = 0; y < height; y++)
        {
            raw[rawIndex++] = 0;
            for (int x = 0; x < width; x++)
            {
                SKColor pixel = bitmap.GetPixel(x, y);
                raw[rawIndex++] = pixel.Red;
                raw[rawIndex++] = pixel.Green;
                raw[rawIndex++] = pixel.Blue;
            }
        }

        using MemoryStream compressedStream = new();
        using (ZLibStream zLibStream = new(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
            zLibStream.Write(raw, 0, raw.Length);
        byte[] compressed = compressedStream.ToArray();

        byte[] ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;
        ihdr[9] = 2;
        ihdr[10] = 0;
        ihdr[11] = 0;
        ihdr[12] = 0;

        using MemoryStream output = new();
        output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], 0, 8);
        WriteChunk(output, "IHDR", ihdr);
        WriteChunk(output, "IDAT", compressed);
        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    /// <summary>
    /// Writes a single length-prefixed, CRC-suffixed PNG chunk to the given stream.
    /// </summary>
    /// <param name="output">The stream to write the chunk to.</param>
    /// <param name="chunkType">The four-character chunk type, e.g. "IHDR".</param>
    /// <param name="data">The chunk's payload.</param>
    private static void WriteChunk(MemoryStream output, string chunkType, byte[] data)
    {
        byte[] length = new byte[4];
        WriteBigEndian(length, 0, data.Length);
        output.Write(length, 0, 4);
        byte[] typeBytes = Encoding.ASCII.GetBytes(chunkType);
        output.Write(typeBytes, 0, 4);
        output.Write(data, 0, data.Length);
        byte[] crcInput = new byte[typeBytes.Length + data.Length];
        Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);
        byte[] crcBytes = new byte[4];
        WriteBigEndian(crcBytes, 0, (int)Crc32(crcInput));
        output.Write(crcBytes, 0, 4);
    }

    /// <summary>
    /// Writes the given value into the buffer at the given offset, as four big-endian bytes.
    /// </summary>
    /// <param name="buffer">The buffer to write into.</param>
    /// <param name="offset">The offset to write at.</param>
    /// <param name="value">The value to write.</param>
    private static void WriteBigEndian(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    /// <summary>
    /// Builds the standard CRC32 lookup table used by the PNG chunk checksum.
    /// </summary>
    private static uint[] BuildCrcTable()
    {
        uint[] table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
        return table;
    }

    /// <summary>
    /// Computes the PNG chunk CRC32 checksum for the given bytes.
    /// </summary>
    /// <param name="data">The chunk type plus payload bytes to checksum.</param>
    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFF;
    }
}