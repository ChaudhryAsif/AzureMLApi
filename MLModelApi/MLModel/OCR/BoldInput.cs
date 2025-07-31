using Microsoft.ML.Data;

namespace MLModelApi.MLModel.OCR
{
    public class BoldInput
    {
        [LoadColumn(0)]
        public string ImagePath;

        [ColumnName("Label")]
        public bool IsBold;
    }
}
