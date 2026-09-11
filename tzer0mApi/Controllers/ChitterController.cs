using Microsoft.AspNetCore.Mvc;
using tzer0mApi.Models.Chitter;
using tzer0mApi.Services.Chitter;

namespace tzer0mApi.Controllers;

/// <summary>
/// Handles print jobs for the Aures ODP 333 receipt printer.
/// </summary>
/// <param name="printService">The service used to render and send print jobs.</param>
/// <param name="quoteService">The service used to pick a random motivational quote or Chinese proverb.</param>
/// <param name="packingListService">The service used to build the packing list text.</param>
[ApiController]
[Route("Chitter")]
public class ChitterController(ChitterPrintService printService, QuoteService quoteService, PackingListService packingListService) : ControllerBase
{
    /// <summary>
    /// The maximum size, in bytes, of an uploaded image.
    /// </summary>
    private const long MaxImageBytes = 15 * 1024 * 1024;

    /// <summary>
    /// Renders the given text (with a divider-and-timestamp footer) and prints it.
    /// </summary>
    /// <param name="text">The plain text to print, sent as the raw request body.</param>
    /// <returns>200 on success, or 502 if the printer could not be reached.</returns>
    [HttpPost("Text", Name = "Print Text")]
    public async Task<IActionResult> PrintText([FromBody] string text)
    {
        // Validate that the request body contains non-empty text.
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest(new { error = "Request body must contain non-empty text." });

        // Validate that the request body does not exceed 1024 characters.
        if (text.Length > 1024)
            return StatusCode(413, new { error = "Request body must not exceed 1024 characters." });

        // Send the text to the print service and check if it was sent successfully.
        bool sent = await printService.PrintTextAsync(text);
        if (!sent)
            return StatusCode(502, new { error = "Failed to reach printer." });

        // Return a success response indicating that the text was sent to the printer.
        return Ok(new { message = "Sent to printer" });
    }

    /// <summary>
    /// Resizes, dithers, and prints the given image.
    /// </summary>
    /// <param name="image">The image file, sent as multipart/form-data under the field name "image".</param>
    /// <returns>200 on success, 400 if the upload is missing/invalid, 413 if it's too large, or 502 if the printer could not be reached.</returns>
    [HttpPost("Image", Name = "Print Image")]
    public async Task<IActionResult> PrintImage(IFormFile? image)
    {
        // Validate that the request contains a non-empty image file.
        if (image is null || image.Length == 0)
            return BadRequest(new { error = "Request must contain a non-empty image file." });

        // Validate that the file does not exceed the maximum upload size.
        if (image.Length > MaxImageBytes)
            return StatusCode(413, new { error = $"Image must not exceed {MaxImageBytes / (1024 * 1024)} MB." });

        // Validate that the file is actually an image, based on its declared content type.
        if (string.IsNullOrEmpty(image.ContentType) || !image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Uploaded file must be an image." });

        // Send the image to the print service and check if it was sent successfully.
        bool sent;
        await using (Stream stream = image.OpenReadStream())
        {
            try
            {
                sent = await printService.PrintImageAsync(stream);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "Could not read the uploaded image - is it a valid image file?" });
            }
        }

        // If the print service failed to send the image to the printer, return a 502 Bad Gateway response.
        if (!sent)
            return StatusCode(502, new { error = "Failed to reach printer." });

        // Return a success response indicating that the image was sent to the printer.
        return Ok(new { message = "Sent to printer" });
    }

    /// <summary>
    /// Picks a random motivational quote and prints it, formatted as the quote followed by its author.
    /// </summary>
    /// <returns>200 on success, or 502 if the printer could not be reached.</returns>
    [HttpPost("Quote", Name = "Print Quote")]
    public async Task<IActionResult> PrintQuote()
    {
        // Pick a random quote and format it for printing.
        MotivationalQuote quote = quoteService.GetRandomQuote();
        string text = $"\"{quote.Quote}\"\n\n— {quote.Author}";

        // Send the text to the print service and check if it was sent successfully.
        bool sent = await printService.PrintTextAsync(text);
        if (!sent)
            return StatusCode(502, new { error = "Failed to reach printer." });

        // Return a success response indicating that the quote was sent to the printer.
        return Ok(new { message = "Sent to printer" });
    }

    /// <summary>
    /// Picks a random Chinese proverb and prints it, with the Chinese text followed by its English translation.
    /// </summary>
    /// <returns>200 on success, or 502 if the printer could not be reached.</returns>
    [HttpPost("Proverb", Name = "Print Proverb")]
    public async Task<IActionResult> PrintProverb()
    {
        // Pick a random proverb and format it for printing.
        ChineseProverb proverb = quoteService.GetRandomProverb();
        string text = $"{proverb.Chinese}\n{proverb.English}";

        // Send the text to the print service and check if it was sent successfully.
        bool sent = await printService.PrintTextAsync(text);
        if (!sent)
            return StatusCode(502, new { error = "Failed to reach printer." });

        // Return a success response indicating that the proverb was sent to the printer.
        return Ok(new { message = "Sent to printer" });
    }

    /// <summary>
    /// Builds and prints the packing list, including the International and/or Skiing sections when requested.
    /// </summary>
    /// <param name="international">Whether to include the International section. Forced true when <paramref name="skiing"/> is true, since skiing trips are always international here.</param>
    /// <param name="skiing">Whether to include the Skiing section.</param>
    /// <returns>200 on success, or 502 if the printer could not be reached.</returns>
    [HttpPost("PackingList", Name = "Print Packing List")]
    public async Task<IActionResult> PrintPackingList([FromQuery] bool international = false, [FromQuery] bool skiing = false)
    {
        // Skiing implies International, regardless of what was passed for it.
        string text = packingListService.BuildText(international || skiing, skiing);

        // Send the text to the print service and check if it was sent successfully.
        bool sent = await printService.PrintTextAsync(text);
        if (!sent)
            return StatusCode(502, new { error = "Failed to reach printer." });

        // Return a success response indicating that the packing list was sent to the printer.
        return Ok(new { message = "Sent to printer" });
    }
}