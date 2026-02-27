using BusinessLogic.Services;
using BusinessLogic.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IImageService _imageService;

        public UploadController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object>(null, "File is missing or empty."));

            var imageUrl = await _imageService.UploadImageAsync(file);

            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest(new ApiResponse<object>(null, "Image upload failed."));

            return Ok(new ApiResponse<string>(imageUrl, "Image uploaded successfully"));
        }

        [HttpDelete("image")]
        public async Task<IActionResult> RemoveImage([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest(new ApiResponse<object>(null, "Url is missing."));

            var success = await _imageService.DeleteImageAsync(url);
            
            if (!success)
                return BadRequest(new ApiResponse<object>(null, "Image deletion failed."));

            return Ok(new ApiResponse<object>(null, "Image deleted successfully"));
        }
    }
}
