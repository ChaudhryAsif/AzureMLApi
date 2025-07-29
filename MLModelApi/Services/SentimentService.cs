using Microsoft.ML;
using MLModelApi.MLModel.Sentiment;

namespace MLModelApi.Services
{
    public class SentimentService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<ReviewInput, ReviewOutput> _predEngine;

        public SentimentService(IWebHostEnvironment env)
        {
            _mlContext = new MLContext();

            var modelPath = Path.Combine(env.ContentRootPath, "Models", "ReviewSentimentModel.zip");
            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"Model file not found: {modelPath}");

            var loadedModel = _mlContext.Model.Load(modelPath, out _);
            _predEngine = _mlContext.Model.CreatePredictionEngine<ReviewInput, ReviewOutput>(loadedModel);
        }

        public (bool prediction, float confidence) Predict(string reviewText)
        {
            var input = new ReviewInput { ReviewText = reviewText };
            var result = _predEngine.Predict(input);

            float confidence = result.Prediction ? result.Probability : 1 - result.Probability;
            return (result.Prediction, confidence);
        }
    }
}
