using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Text;
using System.Text.Json;

namespace MLModelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeechController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SpeechController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("recognize-from-mic")]
        public async Task<IActionResult> RecognizeSpeechFromMic()
        {
            var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
            var region = _configuration["AzureSpeech:Region"];

            var config = SpeechConfig.FromSubscription(subscriptionKey, region);

            using var recognizer = new SpeechRecognizer(config);
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                return Ok(new
                {
                    Text = result.Text,
                    Reason = result.Reason.ToString()
                });
            }
            else
            {
                return BadRequest(new
                {
                    Message = "Speech not recognized",
                    Reason = result.Reason.ToString(),
                    Details = result.Text?.ToString()
                });
            }
        }

        [HttpPost("recognize-from-file")]
        public async Task<IActionResult> RecognizeSpeechFromFile(IFormFile audioFile)
        {
            // Check if a file was uploaded
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("No audio file uploaded.");

            // Create a temporary file path on disk
            var tempFile = Path.GetTempFileName();

            try
            {
                // Save the uploaded file to the temp file location
                using (var stream = new FileStream(tempFile, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
                var region = _configuration["AzureSpeech:Region"];

                // Create a speech config object using Azure Speech subscription and region
                var config = SpeechConfig.FromSubscription(subscriptionKey, region);

                // Load the temp audio file into an audio input config
                var audioInput = AudioConfig.FromWavFileInput(tempFile);

                // Create the speech recognizer with config and audio input
                using var recognizer = new SpeechRecognizer(config, audioInput);

                // Store final recognized text segments
                var finalResult = new List<string>();

                // Event handler: This is called every time speech is successfully recognized
                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        finalResult.Add(e.Result.Text); // Add recognized text to the result list
                    }
                };

                // Used to signal when recognition should stop
                var stopRecognition = new TaskCompletionSource<bool>();

                // Event handler: Called when the recognition session ends normally
                recognizer.SessionStopped += (s, e) =>
                {
                    stopRecognition.TrySetResult(true); // Stop waiting
                };

                // Event handler: Called if recognition is canceled (e.g., error, interruption)
                recognizer.Canceled += (s, e) =>
                {
                    stopRecognition.TrySetResult(true); // Stop waiting on error
                };

                // Start continuous recognition (it starts processing the audio file)
                await recognizer.StartContinuousRecognitionAsync();

                // Wait until session is stopped or canceled
                await stopRecognition.Task;

                // Stop the recognition session after it ends
                await recognizer.StopContinuousRecognitionAsync();

                // Return all recognized text joined together as one string
                return Ok(new { Text = string.Join(" ", finalResult) });
            }
            finally
            {
                // Clean up: delete the temp file regardless of success/failure
                try { System.IO.File.Delete(tempFile); }
                catch { /* Ignore errors in deleting temp file */ }
            }
        }

        /// <summary>
        /// This endpoint receives text and returns a WAV file of the synthesized speech using Azure Cognitive Services
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost("synthesize")]
        public async Task<IActionResult> SynthesizeSpeech([FromBody] string text)
        {
            // Validate input: ensure text is provided
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Text is required");

            // Retrieve Azure Speech configuration values from settings
            var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
            var region = _configuration["AzureSpeech:Region"];

            // Initialize Azure speech configuration with your subscription and region
            var config = SpeechConfig.FromSubscription(subscriptionKey, region);

            // Choose the voice to be used for speech synthesis
            // config.SpeechSynthesisVoiceName = "ur-PK-AsadNeural";
            config.SpeechSynthesisVoiceName = "en-US-JennyNeural"; // English voice used here

            // Create a pull stream to receive the audio output (in-memory WAV data)
            var stream = AudioOutputStream.CreatePullStream();

            // Configure the audio output to write to our in-memory stream
            using var audioOutput = AudioConfig.FromStreamOutput(stream);

            // Create a speech synthesizer that writes to the stream (does NOT play audio)
            using var synthesizer = new SpeechSynthesizer(config, audioOutput);

            // Send the text to Azure Text-to-Speech (TTS) service and receive audio in the stream
            var result = await synthesizer.SpeakTextAsync(text);

            // OPTIONAL: Create another synthesizer that plays audio out loud through the default speaker
            using var audioSynthesizer = new SpeechSynthesizer(config);
            var audio = await audioSynthesizer.SpeakTextAsync(text);

            // If synthesis completed successfully, return the WAV audio as a downloadable file
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                return File(result.AudioData, "audio/wav", "output.wav");
            }
            else
            {
                // If synthesis failed, return the error details
                return BadRequest(new { result.Reason, result });
            }
        }

        // This method translates the input text to Urdu using Azure Translator API.
        [HttpPost("translateToUrdu")]
        public async Task<string> TranslateToUrduAsync([FromBody] JsonElement body)
        {
            // Read Azure Translator credentials from configuration
            var subscriptionKey = _configuration["AzureTranslator:SubscriptionKey"];
            var region = _configuration["AzureTranslator:Region"];

            // Azure Translator endpoint and route for Urdu translation
            var endpoint = "https://api.cognitive.microsofttranslator.com";
            var route = "/translate?api-version=3.0&to=ur"; // 'to=ur' means translate to Urdu

            // Wrap the input JSON body into the expected format for Azure Translator API
            var requestBody = new object[] { new { Text = body } };

            // Serialize the request body into JSON and set content type as application/json
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Create an HttpClient to send the request
            using var client = new HttpClient();

            // Add required Azure Translator API headers (subscription key and region)
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);

            // Make an asynchronous POST request to Azure Translator API
            var response = await client.PostAsync(endpoint + route, content);

            // Read the response body as a string
            var responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize the response into a strongly typed object
            var translationResults = JsonSerializer.Deserialize<List<TranslationResult>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // ignore case of JSON keys
            });

            // Return the translated text, or fallback message if translation failed
            return translationResults?[0].Translations?[0].Text ?? "Translation failed.";
        }

        public class TranslationResult
        {
            public List<TranslationText> Translations { get; set; }
        }

        public class TranslationText
        {
            public string Text { get; set; }
            public string To { get; set; }
        }
    }
}
