using Microsoft.ML;
using MLModelApi.MLModel.OCR;

public class BoldClassifier
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly PredictionEngine<BoldInput, BoldPrediction> _predEngine;

    public BoldClassifier(IConfiguration configuration)
    {
        var modelPath = configuration["BoldModelPath"];

        if (string.IsNullOrWhiteSpace(modelPath))
            throw new ArgumentException("BoldModelPath is not set in configuration.");

        _mlContext = new MLContext();
        _model = _mlContext.Model.Load(modelPath, out _);
        _predEngine = _mlContext.Model.CreatePredictionEngine<BoldInput, BoldPrediction>(_model);
    }

    public BoldPrediction Predict(string croppedImagePath)
    {
        var input = new BoldInput { ImagePath = croppedImagePath };
        return _predEngine.Predict(input);
    }
}
