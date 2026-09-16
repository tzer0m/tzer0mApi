using System.IO.Compression;
using System.Linq;
using System.Text;
using SkiaSharp;
using tzer0mApi.Models.HomeAssistant;

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
            foreach (string line in WrapToLines(calendarEvent.Title, eventTitleFont, allDayColumnMaxWidth, 2))
            {
                canvas.DrawText(line, allDayColumnX, allDayY, SKTextAlign.Left, eventTitleFont, eventFill);
                allDayY += WrapLineHeightPx;
            }
            allDayY += ItemGapPx;
        }
        if (allDayEvents.Count == 0)
            canvas.DrawText("Nothing Scheduled", allDayColumnX, allDayY, SKTextAlign.Left, eventTitleFont, blackFill);

        canvas.DrawRect(new SKRect(dividerOneX - (DividerThicknessPx / 2f), columnTop - BodyTopPaddingPx, dividerOneX + (DividerThicknessPx / 2f), bodyLimitY), blackFill);

        canvas.DrawText("EVENTS", eventsColumnX, columnTop, SKTextAlign.Left, sectionHeaderFont, blackFill);
        float eventTitleX = eventsColumnX + EventTimeColumnWidthPx;
        float eventTitleMaxWidth = dividerTwoX - ColumnTextTrailingPaddingPx - eventTitleX;
        float eventY = columnTop + SectionHeaderGapPx;
        foreach (HomeAssistantEvent calendarEvent in timedEvents)
        {
            if (eventY > bodyLimitY)
                break;
            SKPaint eventFill = GetColorFill(calendarEvent.Color, blackFill, redFill, greenFill, yellowFill, blueFill);
            bool isPast = calendarEvent.End <= now;
            bool isCurrent = !isPast && calendarEvent.Start <= now;
            List<string> titleLines = WrapToLines(calendarEvent.Title, eventTitleFont, eventTitleMaxWidth, 2);
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
        if (timedEvents.Count == 0)
            canvas.DrawText("Nothing Scheduled", eventsColumnX, eventY, SKTextAlign.Left, eventTitleFont, blackFill);

        canvas.DrawRect(new SKRect(dividerTwoX - (DividerThicknessPx / 2f), columnTop - BodyTopPaddingPx, dividerTwoX + (DividerThicknessPx / 2f), bodyLimitY), blackFill);

        canvas.DrawText("TASKS", tasksColumnX, columnTop, SKTextAlign.Left, sectionHeaderFont, blackFill);
        float taskTitleMaxWidth = tasksColumnRight - tasksColumnX;
        float taskY = columnTop + SectionHeaderGapPx;
        foreach (HomeAssistantTask task in tasks)
        {
            if (taskY > bodyLimitY)
                break;
            bool isOverdue = task.Status == HomeAssistantTaskStatus.Overdue;
            List<string> titleLines = WrapToLines(task.Title, taskTitleFont, taskTitleMaxWidth, 2);
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
        if (tasks.Count == 0)
            canvas.DrawText("Nothing Due", tasksColumnX, taskY, SKTextAlign.Left, taskTitleFont, blackFill);

        return EncodeRgbPng(bitmap);
    }

    /// <summary>
    /// Renders a placeholder display, for a display letter without dedicated content yet - the letter and a small caption, in white, over the letter's assigned colour.
    /// </summary>
    /// <param name="letter">The display letter, e.g. "B".</param>
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