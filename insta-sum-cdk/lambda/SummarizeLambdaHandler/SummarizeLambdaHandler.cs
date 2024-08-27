using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace SummarizeLambdaHandler
{
    /// <summary>
    /// This class handles incoming API Gateway requests and processes them to generate summaries, key highlights, or important words
    /// using the OpenAI service.
    /// </summary>
    public class SummarizeLambdaHandler : ISummarizeLambdaHandler
    {
        private readonly IOpenAiService _openAiService;

        /// <summary>
        /// Constructor for SummarizeLambdaHandler. Initializes the handler with the specified OpenAI service.
        /// </summary>
        /// <param name="openAiService">The OpenAI service used for generating summaries and other text-based outputs.</param>
        public SummarizeLambdaHandler(IOpenAiService openAiService)
        {
            _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
        }

        /// <summary>
        /// Handles an API Gateway request asynchronously, routing it based on the request path and processing accordingly.
        /// </summary>
        /// <param name="request">The API Gateway request object.</param>
        /// <param name="context">The Lambda context object.</param>
        /// <returns>An API Gateway response object.</returns>
        public async Task<APIGatewayProxyResponse> HandleAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                ValidateRequest(request, context);

                context.Logger?.LogLine($"Request Path: {request.Path}");
                context.Logger?.LogLine($"Request Body: {request.Body}");

                if (request.HttpMethod != "POST")
                {
                    return Utilities.CreateResponse(405, "Method Not Allowed");
                }

                return request.Path switch
                {
                    "/summary" => await HandleSummaryAsync(request, context),
                    "/key-highlights" => await HandleKeyHighlightsAsync(request, context),
                    "/important-words" => await HandleImportantWordsAsync(request, context),
                    _ => Utilities.CreateResponse(404, "Not Found")
                };
            }
            catch (Exception ex)
            {
                return ErrorHandler.HandleException(ex, context);
            }
        }

        /// <summary>
        /// Validates the API Gateway request and Lambda context to ensure they are not null.
        /// </summary>
        /// <param name="request">The API Gateway request object.</param>
        /// <param name="context">The Lambda context object.</param>
        private void ValidateRequest(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (context == null) throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Handles the "/summary" endpoint by generating a summary of the provided text.
        /// </summary>
        /// <param name="request">The API Gateway request object.</param>
        /// <param name="context">The Lambda context object.</param>
        /// <returns>An API Gateway response containing the generated summary.</returns>
        private async Task<APIGatewayProxyResponse> HandleSummaryAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!TryExtractText(request, out var text, out var errorMessage))
            {
                return Utilities.CreateResponse(400, errorMessage);
            }

            var summary = await _openAiService.CallOpenAiAsync(text, context, "Summarize the given text in a concise manner.");

            if (string.IsNullOrWhiteSpace(summary))
            {
                return Utilities.CreateResponse(400, "Empty response from service");
            }

            return Utilities.CreateResponse(200, new { summary });
        }

        /// <summary>
        /// Handles the "/key-highlights" endpoint by extracting key highlights from the provided text.
        /// </summary>
        /// <param name="request">The API Gateway request object.</param>
        /// <param name="context">The Lambda context object.</param>
        /// <returns>An API Gateway response containing the extracted key highlights.</returns>
        private async Task<APIGatewayProxyResponse> HandleKeyHighlightsAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!TryExtractText(request, out var text, out var errorMessage))
            {
                return Utilities.CreateResponse(400, errorMessage);
            }

            var keyHighlights = await _openAiService.CallOpenAiAsync(text, context, "Extract key highlights from the given text. Return only the bullet points without any additional text or context.");

            return Utilities.CreateResponse(200, new { keyHighlights });
        }

        /// <summary>
        /// Handles the "/important-words" endpoint by extracting important words or phrases from the provided text.
        /// </summary>
        /// <param name="request">The API Gateway request object.</param>
        /// <param name="context">The Lambda context object.</param>
        /// <returns>An API Gateway response containing the extracted important words or phrases.</returns>
        private async Task<APIGatewayProxyResponse> HandleImportantWordsAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!TryExtractText(request, out var text, out var errorMessage))
            {
                return Utilities.CreateResponse(400, errorMessage);
            }

            var importantWords = await _openAiService.CallOpenAiAsync(text, context, "Extract the most important words or phrases from the given text that are central to the main topics. Return only the words or phrases starting with a capital letter, separated by commas, without any additional text.");

            return Utilities.CreateResponse(200, new { importantWords });
        }

        /// <summary>
        /// Attempts to extract text from the API Gateway request, handling potential errors.
        /// </summary>
        /// <param name="request">The API Gateway request object.</param>
        /// <param name="text">The extracted text if successful.</param>
        /// <param name="errorMessage">An error message if text extraction fails.</param>
        /// <returns>A boolean indicating whether text extraction was successful.</returns>
        private bool TryExtractText(APIGatewayProxyRequest request, out string text, out string errorMessage)
        {
            text = null;
            errorMessage = null;

            try
            {
                text = Utilities.ExtractText(request);
                if (string.IsNullOrWhiteSpace(text))
                {
                    errorMessage = "Text parameter is missing";
                    return false;
                }
            }
            catch (JsonReaderException)
            {
                errorMessage = "Invalid JSON format";
                return false;
            }

            return true;
        }
    }
}
