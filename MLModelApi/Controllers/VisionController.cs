using Microsoft.AspNetCore.Mvc;
using MLModelApi.Services;

namespace MLModelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisionController : ControllerBase
    {
        private readonly VisionService _visionService;
        private readonly OcrService _ocr;

        public VisionController(VisionService visionService, OcrService ocr)
        {
            _visionService = visionService;
            _ocr = ocr;
        }

        [HttpPost("ocr")]
        public async Task<IActionResult> ReadText([FromBody] string imageUrl)
        {
            var lines = await _visionService.ExtractTextFromImageAsync(imageUrl);
            return Ok(lines);
        }

        [HttpPost("detect-bold-text")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DetectBoldText(IFormFile image)
        {
            var path = Path.GetTempFileName();
            using var stream = System.IO.File.Create(path);
            await image.CopyToAsync(stream);

            var result = await _ocr.ExtractTextWithBoldAsync(path);
            return Ok(result.Select(r => new { Text = r.Text, IsBold = r.IsBold }));
        }
    }
}
