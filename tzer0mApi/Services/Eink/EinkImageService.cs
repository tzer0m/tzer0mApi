using System.IO.Compression;
using System.Text;
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
    /// Font size, in points, used for a placeholder display's big letter.
    /// </summary>
    private const float PlaceholderLetterFontSize = 260f;

    /// <summary>
    /// Font size, in points, used for a placeholder display's caption.
    /// </summary>
    private const float PlaceholderCaptionFontSize = 40f;

    /// <summary>
    /// The lookup table used for PNG chunk CRC32 checksums.
    /// </summary>
    private static readonly uint[] CrcTable = BuildCrcTable();

    /// <summary>
    /// Renders the current time and date to an 800x480 PNG, for display A.
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