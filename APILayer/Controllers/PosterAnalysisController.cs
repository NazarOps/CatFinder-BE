using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace APILayer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PosterAnalysisController : ControllerBase
    {
        private const long MaxUploadBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp",
            "image/gif"
        };

        private readonly IConfiguration _config;

        public PosterAnalysisController(IConfiguration config)
        {
            _config = config;
        }

        [EnableRateLimiting("uploads")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Analyze(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "Ingen bild angiven" });

            if (image.Length > MaxUploadBytes)
                return BadRequest(new { error = "Bilden får vara högst 5 MB." });

            if (!AllowedContentTypes.Contains(image.ContentType ?? string.Empty))
                return BadRequest(new { error = "Filformat stöds ej." });

            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY")
                return StatusCode(503, new { error = "OpenAI API-nyckel ej konfigurerad" });

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var imageBytes = ms.ToArray();
            var mimeType = image.ContentType ?? "image/jpeg";

            const string prompt = """
                This is a Swedish lost/found cat poster. Extract as much information as possible and return ONLY a JSON object with these exact fields (use null for any field you cannot find):
                {
                  "title": "a short advertisement title in Swedish (e.g. 'Försvunnen katt i Södermalm')",
                  "description": "detailed description based on the poster content in Swedish",
                  "type": "Lost" or "Found",
                  "catName": "the cat's name or null",
                  "catBreed": "the cat's breed or null",
                  "catFurColor": "choose the closest match from: Svart, Vit, Grå, Orange, Brun, Beige, Rödbrun, Blågrå, Calico — or null if unclear",
                  "city": "the Swedish city name (must be one of the major Swedish cities) or null",
                  "contactPhoneNumber": "phone number found on the poster or null",
                  "contactEmail": "email address found on the poster or null"
                }
                Return ONLY valid JSON with no markdown formatting, no code fences, no explanation.
                """;

            try
            {
                var client = new ChatClient("gpt-4o", new ApiKeyCredential(apiKey));

                var response = await client.CompleteChatAsync(
                [
                    new UserChatMessage(
                        ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType),
                        ChatMessageContentPart.CreateTextPart(prompt)
                    )
                ]);

                var raw = response.Value.Content[0].Text ?? "{}";

                var match = Regex.Match(raw, @"\{[\s\S]*\}");
                var cleanJson = match.Success ? match.Value : "{}";

                try
                {
                    using var doc = JsonDocument.Parse(cleanJson);
                    return Ok(doc.RootElement);
                }
                catch
                {
                    return Ok(new { });
                }
            }
            catch (ClientResultException ex)
            {
                return StatusCode((int)(ex.Status == 0 ? 502 : ex.Status), new { error = ex.Message });
            }
        }
    }
}
