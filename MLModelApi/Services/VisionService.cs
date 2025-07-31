using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace MLModelApi.Services
{
    public class VisionService
    {
        private readonly ComputerVisionClient _client;

        public VisionService(IConfiguration config)
        {
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(config["AzureVision:Key"]))
            {
                Endpoint = config["AzureVision:Endpoint"]
            };
        }

        public async Task<List<string>> ExtractTextFromImageAsync(string localPath)
        {
            using var stream = File.OpenRead(localPath);
            var result = await _client.ReadInStreamAsync(stream);

            //var result = await _client.ReadAsync(imageUrl);
            string operationLocation = result.OperationLocation;
            string operationId = operationLocation[^36..];

            ReadOperationResult readResult;
            do
            {
                await Task.Delay(1000);
                readResult = await _client.GetReadResultAsync(Guid.Parse(operationId));
            } while (readResult.Status == OperationStatusCodes.Running || readResult.Status == OperationStatusCodes.NotStarted);

            var lines = new List<string>();
            if (readResult.Status == OperationStatusCodes.Succeeded)
            {
                foreach (var page in readResult.AnalyzeResult.ReadResults)
                    lines.AddRange(page.Lines.Select(l => l.Text));
            }
            return lines;
        }
    }
}
