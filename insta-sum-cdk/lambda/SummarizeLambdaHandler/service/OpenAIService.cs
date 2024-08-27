using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon;

namespace SummarizeLambdaHandler
{
    /// <summary>
    /// The OpenAiService class is responsible for interacting with the OpenAI API.
    /// It retrieves the API key from AWS Secrets Manager, sends a request to OpenAI, and returns the response.
    /// </summary>
    public class OpenAiService : IOpenAiService
    {
        // Static HttpClient instance for sending HTTP requests
        private static readonly HttpClient _httpClient = new HttpClient();

        // Environment variables for retrieving the OpenAI API key and AWS region
        private static readonly string SecretName = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        private static readonly string RegionName = Environment.GetEnvironmentVariable("AWS_REGION") ?? "ap-southeast-2";
        private static readonly RegionEndpoint Region = RegionEndpoint.GetBySystemName(RegionName);

        /// <summary>
        /// Calls the OpenAI API with the provided text and prompt, and returns the generated summary.
        /// </summary>
        /// <param name="text">The text to be summarized by OpenAI.</param>
        /// <param name="context">The Lambda context for logging and runtime information.</param>
        /// <param name="prompt">The prompt to guide OpenAI's response.</param>
        /// <returns>The summarized text returned by OpenAI.</returns>
        public async Task<string> CallOpenAiAsync(string text, ILambdaContext context, string prompt)
        {
            context.Logger.LogLine("Start CallOpenAiAsync.");

            // Retrieve the OpenAI API key from AWS Secrets Manager
            var apiKey = await AwsSecretsManagerUtils.GetSecretAsync(SecretName, Region, context.Logger);
            context.Logger.LogLine("Retrieved API Key.");

            // Prepare the request to OpenAI with the necessary parameters
            var apiEndpoint = "https://api.openai.com/v1/chat/completions";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = $"{prompt}\n\nText: {text}" }
                },
                max_tokens = 200
            };

            // Serialize the request body into JSON format
            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            // Set the Authorization header with the API key
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Send the request to OpenAI and get the response
            var response = await _httpClient.PostAsync(apiEndpoint, jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            context.Logger.LogLine("OpenAI response received.");

            // Check if the response was successful and return the result
            if (response.IsSuccessStatusCode)
            {
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                return jsonResponse.choices[0].message.content.ToString();
            }
            else
            {
                throw new Exception("Failed to get a response from OpenAI.");
            }
        }
    }
}
