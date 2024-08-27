using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SummarizeLambdaHandler
{
    /// <summary>
    /// The LambdaEntryPoint class serves as the entry point for AWS Lambda function invocations.
    /// It initializes the necessary services and delegates the handling of requests to the appropriate handler.
    /// </summary>
    public class LambdaEntryPoint
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SummarizeLambdaHandler _handler;

        /// <summary>
        /// Default constructor used by AWS Lambda. Initializes the service provider and handler.
        /// </summary>
        public LambdaEntryPoint()
        {
            // Initialize services manually if no dependency injection is provided.
            _serviceProvider = ConfigureServices();
            _handler = _serviceProvider.GetService<SummarizeLambdaHandler>();
        }

        /// <summary>
        /// Constructor used for unit testing or when dependency injection is available.
        /// </summary>
        /// <param name="serviceProvider">The service provider containing the registered services.</param>
        public LambdaEntryPoint(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _handler = _serviceProvider.GetService<SummarizeLambdaHandler>() 
                ?? throw new InvalidOperationException("SummarizeLambdaHandler service is not registered.");
        }

        /// <summary>
        /// Configures the services for dependency injection. 
        /// Registers necessary services like OpenAiService and SummarizeLambdaHandler.
        /// </summary>
        /// <returns>An IServiceProvider with all the registered services.</returns>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddTransient<IOpenAiService, OpenAiService>();
            services.AddTransient<SummarizeLambdaHandler>();
            return services.BuildServiceProvider();
        }

        /// <summary>
        /// The function handler that AWS Lambda will invoke. Processes the incoming API Gateway request.
        /// </summary>
        /// <param name="request">The API Gateway proxy request containing the HTTP request data.</param>
        /// <param name="context">The Lambda context providing information about the Lambda environment.</param>
        /// <returns>A Task representing the asynchronous operation, with an APIGatewayProxyResponse as the result.</returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                // Delegate the handling of the request to the SummarizeLambdaHandler.
                var result = await _handler.HandleAsync(request, context);

                // Return a successful response with the result.
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(new { summary = result }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            catch (Exception ex)
            {
                // Log any errors encountered during processing.
                context.Logger.LogError($"Error: {ex.Message}");

                // Return an error response with details of the exception.
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = JsonConvert.SerializeObject(new { message = "Internal server error", error = ex.Message }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        }
    }
}
