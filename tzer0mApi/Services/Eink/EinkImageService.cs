using SkiaSharp;

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
    /// Renders the current time and date to an 800x480 PNG, for display A.
    /// </summary>
    /// <returns>The rendered image, encoded as PNG bytes.</returns>
    public byte[] RenderClock()
    {
        using SKTypeface boldTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Bold.ttf");
        using SKTypeface mediumTypeface = LoadTypeface("Assets/Fonts/SpaceGrotesk-Medium.ttf");
        using SKFont timeFont = new(boldTypeface, TimeFontSize);
        using SKFont dateFont = new(mediumTypeface, DateFontSize);
        using SKPaint paint = new() { Color = SKColors.Black, IsAntialias = true };

        DateTime now = DateTime.Now;
        string timeText = now.ToString("HH:mm:ss");
        string dateText = now.ToString("dddd d MMMM yyyy");

        using SKBitmap bitmap = new(WidthPx, HeightPx);
        bitmap.Erase(SKColors.White);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.DrawText(timeText, WidthPx / 2f, HeightPx / 2f, SKTextAlign.Center, timeFont, paint);
            canvas.DrawText(dateText, WidthPx / 2f, (HeightPx / 2f) + 70f, SKTextAlign.Center, dateFont, paint);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return StripAncillaryChunks(data.ToArray());
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
    /// Strips ancillary PNG chunks (e.g. sBIT, gAMA), keeping only IHDR, PLTE, tRNS, IDAT, and IEND - some embedded decoders don't tolerate chunk types they don't recognise.
    /// </summary>
    /// <param name="png">The original PNG bytes.</param>
    /// <returns>The PNG bytes with only the essential chunks retained.</returns>
    private static byte[] StripAncillaryChunks(byte[] png)
    {
        HashSet<string> essentialChunkTypes = ["IHDR", "PLTE", "tRNS", "IDAT", "IEND"];
        using MemoryStream output = new();
        output.Write(png, 0, 8);
        int position = 8;
        while (position < png.Length)
        {
            int length = (png[position] << 24) | (png[position + 1] << 16) | (png[position + 2] << 8) | png[position + 3];
            string chunkType = System.Text.Encoding.ASCII.GetString(png, position + 4, 4);
            int chunkTotalLength = 12 + length;
            if (essentialChunkTypes.Contains(chunkType))
                output.Write(png, position, chunkTotalLength);
            position += chunkTotalLength;
        }
        return output.ToArray();
    }
}