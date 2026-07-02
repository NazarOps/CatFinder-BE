using ApplicationLayer.AdvertisementImages.Commands.CreateAdvertisementImage;
using ApplicationLayer.AdvertisementImages.Commands.DeleteAdvertisementImage;
using ApplicationLayer.AdvertisementImages.DTOs;
using ApplicationLayer.AdvertisementImages.Queries.GetByAdvertisement;
using ApplicationLayer.AdvertisementImages.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace APILayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisementImagesController : ControllerBase
    {
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

        [HttpPost]
        [ProducesResponseType(typeof(AdvertisementImageResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAdvertisementImageDto dto)
        {
            var result = await _mediator.Send(new CreateAdvertisementImageCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            if (result.Data is not null)
                result.Data.ImageUrl = ToAbsoluteImageUrl(result.Data.ImageUrl);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.AdvertisementImageId }, result);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AdvertisementImageResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] int advertisementId, [FromForm] bool isPrimary = false)
        {
            string[] allowed = ["image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"];
            if (!allowed.Contains(file.ContentType))
                return BadRequest("Filformat stöds ej.");

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);

            await using var stream = System.IO.File.Create(Path.Combine(uploadsDir, fileName));
            await file.CopyToAsync(stream);

            var imageUrl = $"/uploads/{fileName}";
            var dto = new CreateAdvertisementImageDto
            {
                AdvertisementId = advertisementId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
            };

            var result = await _mediator.Send(new CreateAdvertisementImageCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            if (result.Data is not null)
                result.Data.ImageUrl = ToAbsoluteImageUrl(result.Data.ImageUrl);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.AdvertisementImageId }, result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteAdvertisementImageCommand(id));
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
    }
}
