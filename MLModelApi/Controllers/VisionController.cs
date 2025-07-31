using Microsoft.AspNetCore.Mvc;
using MLModelApi.Services;

namespace MLModelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisionController : ControllerBase
    {
        private readonly VisionService _visionService;

        public VisionController(VisionService visionService)
        {
            _visionService = visionService;
        }

        [HttpPost("ocr")]
        public async Task<IActionResult> ReadText([FromBody] string imageUrl)
        {
            var lines = await _visionService.ExtractTextFromImageAsync(imageUrl);
            return Ok(lines);
        }
    }
}
