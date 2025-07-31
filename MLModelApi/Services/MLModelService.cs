using Microsoft.ML;
using MLModelApi.MLModel;

namespace MLModelApi.Services
{
    public class MLModelService
    {
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        public MLModelService(IWebHostEnvironment env)
        {
            var mlContext = new MLContext();
            var modelPath = Path.Combine(env.ContentRootPath, "MLModel.zip");

            using var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var model = mlContext.Model.Load(stream, out _);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        }

        public float Predict(ModelInput input)
        {
            return _predictionEngine.Predict(input).Score;
        }
    }
}
