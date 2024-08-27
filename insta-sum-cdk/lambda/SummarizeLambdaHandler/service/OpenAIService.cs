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
    public class OpenAiService : IOpenAiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string SecretName = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        private static readonly string RegionName = Environment.GetEnvironmentVariable("AWS_REGION") ?? "ap-southeast-2";
        private static readonly RegionEndpoint Region = RegionEndpoint.GetBySystemName(RegionName);

        public async Task<string> CallOpenAiAsync(string text, ILambdaContext context, string prompt)
        {
            context.Logger.LogLine("Start CallOpenAiAsync.");

            // Retrieve the OpenAI API key from AWS Secrets Manager
            var apiKey = await AwsSecretsManagerUtils.GetSecretAsync(SecretName, Region, context.Logger);
            context.Logger.LogLine("Retrieved API Key.");

            // Prepare the request to OpenAI
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Send the request to OpenAI
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
