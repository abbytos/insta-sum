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
    public class LambdaEntryPoint
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SummarizeLambdaHandler _handler;

        // Parameterless constructor for Lambda invocation
        public LambdaEntryPoint()
        {
            // Fallback to manual service initialization if no DI is provided
            _serviceProvider = ConfigureServices();
            _handler = _serviceProvider.GetService<SummarizeLambdaHandler>();
        }

        // Constructor for unit testing or dependency injection
        public LambdaEntryPoint(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _handler = _serviceProvider.GetService<SummarizeLambdaHandler>() 
                ?? throw new InvalidOperationException("SummarizeLambdaHandler service is not registered.");
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddTransient<IOpenAiService, OpenAiService>();
            services.AddTransient<SummarizeLambdaHandler>();
            return services.BuildServiceProvider();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var result = await _handler.HandleAsync(request, context);

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(new { summary = result }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");

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
