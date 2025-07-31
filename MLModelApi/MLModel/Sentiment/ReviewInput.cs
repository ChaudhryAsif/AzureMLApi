using Microsoft.ML.Data;

namespace MLModelApi.MLModel.Sentiment
{
    public class ReviewInput
    {
        [LoadColumn(0)]
        public bool Label { get; set; }

        [LoadColumn(1)]
        public string ReviewText { get; set; } = string.Empty;
    }
}
