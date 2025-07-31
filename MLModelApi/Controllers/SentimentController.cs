using Microsoft.AspNetCore.Mvc;
using MLModelApi.Services;

namespace MLModelApi.Controllers
{
    public class SentimentController : ControllerBase
    {
        private readonly SentimentService _sentimentService;

        public SentimentController(SentimentService sentimentService)
        {
            _sentimentService = sentimentService;
        }

        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] string reviewText)
        {
            if (string.IsNullOrWhiteSpace(reviewText))
                return BadRequest("Review text cannot be empty.");

            var (prediction, confidence) = _sentimentService.Predict(reviewText);
            return Ok(new
            {
                Sentiment = prediction ? "Positive" : "Negative",
                Confidence = confidence
            });
        }
    }
}
