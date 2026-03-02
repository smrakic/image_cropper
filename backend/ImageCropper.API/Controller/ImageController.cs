using Microsoft.AspNetCore.Mvc;
using ImageCropper.API.PomocneController;
using Microsoft.Data.SqlClient;

namespace ImageCropper.API.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        private readonly IPomocnaZaProcesiranjeSlike _imageProcessing;
        private readonly SqlConnection _connection;

        public ImageController(IPomocnaZaProcesiranjeSlike imageProcessing, SqlConnection connection)
        {
            _imageProcessing = imageProcessing;
            _connection = connection;
        }

        [HttpPost("preview")]
        public async Task<IActionResult> Preview(
            [FromForm] IFormFile imageData,
            [FromForm] int x,
            [FromForm] int y,
            [FromForm] int width,
            [FromForm] int height)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return BadRequest("Image data is required");

                using var ms = new MemoryStream();
                await imageData.CopyToAsync(ms);
                var bytes = ms.ToArray();

                var croppedImage = _imageProcessing.CropImage(
                    bytes, x, y, width, height, scale: 0.05f
                );

                return File(croppedImage, "image/png");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Finalno(
            [FromForm] IFormFile imageData,
            [FromForm] int x,
            [FromForm] int y,
            [FromForm] int width,
            [FromForm] int height)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return BadRequest("Image data is required");

                using var ms = new MemoryStream();
                await imageData.CopyToAsync(ms);
                var bytes = ms.ToArray();

                var croppedImage = _imageProcessing.CropImage(
                    bytes, x, y, width, height
                );

                // Provjeri postoji li config za logo overlay
                var config = await ConfigController.GetLatestConfig(_connection);
                
                if (config.LogoImage != null)
                {
                    croppedImage = _imageProcessing.ApplyLogo(
                        croppedImage,
                        config.LogoImage,
                        config.LogoPosition,
                        config.ScaleDown
                    );
                }

                return File(croppedImage, "image/png");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}