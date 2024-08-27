namespace SummarizeLambdaHandler.Test
{
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.APIGatewayEvents;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class ApiGatewayResponse
    {
        public int StatusCode { get; set; }
        public object Headers { get; set; }
        public object MultiValueHeaders { get; set; }
        public string Body { get; set; }
        public bool IsBase64Encoded { get; set; }
    }

    public class SummaryResponse
    {
        public string Summary { get; set; }
    }

    public class OuterResponse
    {
        public ApiGatewayResponse Summary { get; set; }
    }


    public class LambdaEntryPointTests
    {
        private readonly LambdaEntryPoint _lambdaEntryPoint;
        private readonly Mock<ILambdaContext> _mockLambdaContext;
        private readonly Mock<IOpenAiService> _mockOpenAiService;

        public LambdaEntryPointTests()
        {
            // Set environment variables
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "your-mocked-api-key");
            Environment.SetEnvironmentVariable("AWS_REGION", "ap-southeast-2");

            // Ensure environment variables are set
            ValidateEnvironmentVariables();

            // Mock the OpenAiService
            _mockOpenAiService = new Mock<IOpenAiService>();
            _mockOpenAiService
                .Setup(service => service.CallOpenAiAsync(It.IsAny<string>(), It.IsAny<ILambdaContext>(), It.IsAny<string>()))
                .ReturnsAsync("Mocked AI response");

            // Setup the service collection and inject the mock service
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_mockOpenAiService.Object);  // Inject the mock service
            serviceCollection.AddTransient<SummarizeLambdaHandler>();  // Register handler

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Inject serviceProvider into LambdaEntryPoint
            _lambdaEntryPoint = new LambdaEntryPoint(serviceProvider);

            // Mock Lambda Context
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockLambdaContext.Setup(ctx => ctx.Logger).Returns(new Mock<ILambdaLogger>().Object);
            _mockLambdaContext.Setup(ctx => ctx.AwsRequestId).Returns("mocked-request-id");
        }

        private void ValidateEnvironmentVariables()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(region))
            {
                throw new InvalidOperationException("Environment variables are not set correctly.");
            }
        }

        [Fact]
        public async Task FunctionHandler_ReturnsExpectedResponse()
        {
            // Arrange
            var request = new APIGatewayProxyRequest
            {
                HttpMethod = "POST",
                Path = "/summary",
                Body = JsonConvert.SerializeObject(new { text = "Some text." })
            };

            // Act
            var response = await _lambdaEntryPoint.FunctionHandler(request, _mockLambdaContext.Object);

            // Assert
            Assert.Equal(200, response.StatusCode);

            // Deserialize the response body
            var outerResponse = JsonConvert.DeserializeObject<OuterResponse>(response.Body);
            Assert.NotNull(outerResponse);

            // Deserialize the body content
            var bodyJsonString = outerResponse.Summary.Body;
            var innerResponse = JsonConvert.DeserializeObject<SummaryResponse>(bodyJsonString);
            Assert.NotNull(innerResponse);

            // Assert the expected value
            Assert.Equal("Mocked AI response", innerResponse.Summary);
        }

    }
}
