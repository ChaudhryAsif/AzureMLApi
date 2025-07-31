using Microsoft.ML.Data;

namespace MLModelApi.MLModel.OCR
{
    public class BoldPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsBold;
        public float Probability;
        public float Score;
    }
}
