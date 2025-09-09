using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; // REQUIRED for .Crop()

namespace MLModelApi.Services
{
    public class OcrService
    {
        private readonly ComputerVisionClient _client;
        private readonly BoldClassifier _classifier;

        public OcrService(ComputerVisionClient client, BoldClassifier classifier)
        {
            _client = client;
            _classifier = classifier;
        }

        public async Task<List<(string Text, bool IsBold)>> ExtractTextWithBoldAsync(string imagePath)
        {
            var results = new List<(string, bool)>();
            using var stream = File.OpenRead(imagePath);

            var readResponse = await _client.ReadInStreamAsync(stream);
            string operationId = readResponse.OperationLocation[^36..];

            ReadOperationResult result;
            do
            {
                await Task.Delay(1000);
                result = await _client.GetReadResultAsync(Guid.Parse(operationId));
            } while (result.Status == OperationStatusCodes.Running || result.Status == OperationStatusCodes.NotStarted);

            using var image = Image.Load<Rgba32>(imagePath);

            foreach (var page in result.AnalyzeResult.ReadResults)
            {
                foreach (var line in page.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        string croppedPath = CropWord(image, word.BoundingBox, word.Text);
                        var prediction = _classifier.Predict(croppedPath);
                        results.Add((word.Text, prediction.IsBold));
                        File.Delete(croppedPath);
                    }
                }
            }
            return results;
        }

        private string CropWord(Image<Rgba32> image, IList<double?> bbox, string word)
        {
            if (bbox == null || bbox.Count < 8 || bbox.Any(b => b == null))
                throw new ArgumentException("Invalid bounding box coordinates.");

            double x1 = bbox[0].Value;
            double y1 = bbox[1].Value;
            double x2 = bbox[2].Value;
            double y2 = bbox[5].Value;

            int xMin = (int)Math.Min(x1, x2);
            int yMin = (int)Math.Min(y1, y2);
            int width = (int)Math.Abs(x2 - x1);
            int height = (int)Math.Abs(y2 - y1);

            width = Math.Clamp(width, 1, image.Width - xMin);
            height = Math.Clamp(height, 1, image.Height - yMin);

            // ✅ This will work now
            var crop = image.Clone(ctx => ctx.Crop(new Rectangle(xMin, yMin, width, height)));

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            crop.Save(tempPath);
            return tempPath;
        }

    }
}
