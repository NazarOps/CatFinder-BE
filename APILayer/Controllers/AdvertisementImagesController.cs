using ApplicationLayer.AdvertisementImages.Commands.CreateAdvertisementImage;
using ApplicationLayer.AdvertisementImages.Commands.DeleteAdvertisementImage;
using ApplicationLayer.AdvertisementImages.DTOs;
using ApplicationLayer.AdvertisementImages.Queries.GetByAdvertisement;
using ApplicationLayer.AdvertisementImages.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace APILayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisementImagesController : ControllerBase
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

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif"
        };

        private readonly ISender _mediator;

        public AdvertisementImagesController(ISender mediator) => _mediator = mediator;

        [HttpGet("advertisement/{advertisementId:int}")]
        [ProducesResponseType(typeof(IEnumerable<AdvertisementImageResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAdvertisement(int advertisementId)
        {
            var result = await _mediator.Send(new GetAdvertisementImagesByAdvertisementQuery(advertisementId));
            if (result.Data is not null)
            {
                foreach (var image in result.Data)
                {
                    image.ImageUrl = ToAbsoluteImageUrl(image.ImageUrl);
                }
            }

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AdvertisementImageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetAdvertisementImageByIdQuery(id));
            if (!result.IsSuccess) return NotFound(result);
            if (result.Data is not null)
                result.Data.ImageUrl = ToAbsoluteImageUrl(result.Data.ImageUrl);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(AdvertisementImageResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAdvertisementImageDto dto)
        {
            var result = await _mediator.Send(new CreateAdvertisementImageCommand(dto));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess && result.Errors.Contains("Advertisement not found."))
                return NotFound(result);
            if (!result.IsSuccess) return BadRequest(result);
            if (result.Data is not null)
                result.Data.ImageUrl = ToAbsoluteImageUrl(result.Data.ImageUrl);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.AdvertisementImageId }, result);
        }

        [Authorize]
        [EnableRateLimiting("uploads")]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AdvertisementImageResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] int advertisementId, [FromForm] bool isPrimary = false)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No image file was provided.");

            if (file.Length > MaxUploadBytes)
                return BadRequest("The image exceeds the 5 MB upload limit.");

            if (!IsAllowedImage(file))
                return BadRequest("Unsupported or invalid image file.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var imageUrl = $"/uploads/{fileName}";

            var dto = new CreateAdvertisementImageDto
            {
                AdvertisementId = advertisementId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
            };

            var result = await _mediator.Send(new CreateAdvertisementImageCommand(dto));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess && result.Errors.Contains("Advertisement not found."))
                return NotFound(result);
            if (!result.IsSuccess) return BadRequest(result);

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);
            var filePath = Path.Combine(uploadsDir, fileName);

            try
            {
                await using var stream = System.IO.File.Create(filePath);
                await file.CopyToAsync(stream);
            }
            catch
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                if (result.Data is not null)
                    await _mediator.Send(new DeleteAdvertisementImageCommand(result.Data.AdvertisementImageId));

                throw;
            }

            if (result.Data is not null)
                result.Data.ImageUrl = ToAbsoluteImageUrl(result.Data.ImageUrl);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.AdvertisementImageId }, result);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteAdvertisementImageCommand(id));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return NotFound(result);
            return NoContent();
        }

        private string ToAbsoluteImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return imageUrl;

            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                return imageUrl;

            var normalizedPath = imageUrl.StartsWith('/') ? imageUrl : $"/{imageUrl}";
            return $"{Request.Scheme}://{Request.Host}{normalizedPath}";
        }

        private static bool IsAllowedImage(IFormFile file)
        {
            var contentType = file.ContentType ?? string.Empty;
            if (!AllowedContentTypes.Contains(contentType))
                return false;

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                return false;

            using var stream = file.OpenReadStream();
            return HasKnownSignature(stream, extension);
        }

        private static bool HasKnownSignature(Stream stream, string extension)
        {
            Span<byte> header = stackalloc byte[12];
            var bytesRead = stream.Read(header);

            if (bytesRead < 4)
                return false;

            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3
                    && header[0] == 0xFF
                    && header[1] == 0xD8
                    && header[2] == 0xFF,
                ".png" => bytesRead >= 8
                    && header[0] == 0x89
                    && header[1] == 0x50
                    && header[2] == 0x4E
                    && header[3] == 0x47
                    && header[4] == 0x0D
                    && header[5] == 0x0A
                    && header[6] == 0x1A
                    && header[7] == 0x0A,
                ".gif" => bytesRead >= 6
                    && header[0] == 0x47
                    && header[1] == 0x49
                    && header[2] == 0x46
                    && header[3] == 0x38
                    && (header[4] == 0x37 || header[4] == 0x39)
                    && header[5] == 0x61,
                ".webp" => bytesRead >= 12
                    && header[0] == 0x52
                    && header[1] == 0x49
                    && header[2] == 0x46
                    && header[3] == 0x46
                    && header[8] == 0x57
                    && header[9] == 0x45
                    && header[10] == 0x42
                    && header[11] == 0x50,
                _ => false
            };
        }
    }
}
