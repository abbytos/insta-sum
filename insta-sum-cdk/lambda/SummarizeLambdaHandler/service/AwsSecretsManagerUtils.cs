using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SummarizeLambdaHandler
{
    public static class AwsSecretsManagerUtils
    {
        /// <summary>
        /// Retrieves a secret from AWS Secrets Manager.
        /// </summary>
        /// <param name="secretArn">The ARN of the secret to retrieve.</param>
        /// <param name="region">The AWS region where the secret is stored.</param>
        /// <param name="logger">The logger for logging messages.</param>
        /// <returns>The secret value as a string.</returns>
        /// <exception cref="ArgumentException">Thrown if the secretArn is null or empty.</exception>
        /// <exception cref="Exception">Thrown if there is an error retrieving or parsing the secret.</exception>
        public static async Task<string> GetSecretAsync(string secretArn, RegionEndpoint region, ILambdaLogger logger)
        {
            if (string.IsNullOrWhiteSpace(secretArn))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretArn));
            }

            var client = new AmazonSecretsManagerClient(region);
            var request = new GetSecretValueRequest
            {
                SecretId = secretArn, // Use the secret ARN here
                VersionStage = "AWSCURRENT" // Default to the current version
            };

            try
            {
                logger.LogLine($"Attempting to retrieve secret: {secretArn}");
                var response = await client.GetSecretValueAsync(request);
                logger.LogLine("Secret retrieved successfully.");

                // Assuming the secret is a JSON string with a key "OPENAI_API_KEY"
                return ExtractApiKeyFromJson(response.SecretString, logger);
            }
            catch (ResourceNotFoundException ex)
            {
                logger.LogLine($"Secret {secretArn} not found.");
                throw new Exception($"The requested secret {secretArn} was not found", ex);
            }
            catch (Exception ex)
            {
                logger.LogLine($"Error retrieving the secret from AWS Secrets Manager: {ex.Message}");
                throw new Exception("Error retrieving the secret from AWS Secrets Manager.", ex);
            }
        }

        /// <summary>
        /// Extracts the API key from a JSON string.
        /// </summary>
        /// <param name="jsonString">The JSON string containing the secret.</param>
        /// <param name="logger">The logger for logging messages.</param>
        /// <returns>The API key extracted from the JSON string.</returns>
        /// <exception cref="Exception">Thrown if the JSON string is invalid or the key is not found.</exception>
        private static string ExtractApiKeyFromJson(string secretString, ILambdaLogger logger)
        {
            try
            {
                var secretDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretString);
                if (secretDictionary != null && secretDictionary.TryGetValue("OPENAI_API_KEY", out var apiKey))
                {
                    return apiKey;
                }

                throw new Exception("API key not found in secret.");
            }
            catch (JsonException ex)
            {
                logger.LogLine($"Error parsing secret JSON: {ex.Message}");
                throw new Exception("Error parsing secret JSON.", ex);
            }
        }

    }
}
