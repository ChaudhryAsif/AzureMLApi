using Microsoft.AspNetCore.Mvc;
using MLModelApi.MLModel;
using MLModelApi.Services;

namespace MLModelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly MLModelService _modelService;

        public PredictionController(MLModelService modelService)
        {
            _modelService = modelService;
        }

        [HttpPost]
        public IActionResult Predict([FromBody] ModelInput input)
        {
            var result = _modelService.Predict(input);
            return Ok(new { PredictedCount = result });
        }
    }
}
