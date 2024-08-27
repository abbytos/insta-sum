namespace SummarizeLambdaHandler.Test
{
    using System;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.APIGatewayEvents;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    public class SummarizeLambdaHandlerTests
    {
        private readonly Mock<IOpenAiService> _mockOpenAiService;
        private SummarizeLambdaHandler _handler;

        public SummarizeLambdaHandlerTests()
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

            _handler = new SummarizeLambdaHandler(_mockOpenAiService.Object);
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

        private Mock<ILambdaContext> CreateMockLambdaContext()
        {
            var mockContext = new Mock<ILambdaContext>();
            mockContext.Setup(ctx => ctx.Logger).Returns(new Mock<ILambdaLogger>().Object);
            mockContext.Setup(ctx => ctx.AwsRequestId).Returns("mocked-request-id");
            return mockContext;
        }

        [Fact]
        public async Task HandleSummaryAsync_ShouldReturnSummary_WhenTextIsProvided()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/summary", "This is a test input for summary.");

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(200, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Mocked AI response", (string)responseBody.summary);
        }

        [Fact]
        public async Task HandleKeyHighlightsAsync_ShouldReturnKeyHighlights_WhenTextIsProvided()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/key-highlights", "This is a test input for key highlights.");
            _mockOpenAiService
                .Setup(service => service.CallOpenAiAsync(It.IsAny<string>(), It.IsAny<ILambdaContext>(), "Extract key highlights from the given text. Return only the bullet points without any additional text or context."))
                .ReturnsAsync("Key highlight 1\nKey highlight 2");

            _handler = new SummarizeLambdaHandler(_mockOpenAiService.Object);

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(200, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Key highlight 1\nKey highlight 2", (string)responseBody.keyHighlights);
        }

        [Fact]
        public async Task HandleImportantWordsAsync_ShouldReturnImportantWords_WhenTextIsProvided()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/important-words", "This is a test input for extracting important words.");
            _mockOpenAiService
                .Setup(service => service.CallOpenAiAsync(It.IsAny<string>(), It.IsAny<ILambdaContext>(), "Extract the most important words or phrases from the given text that are central to the main topics. Return only the words or phrases starting with a capital letter, separated by commas, without any additional text."))
                .ReturnsAsync("Important Word 1, Important Word 2");

            _handler = new SummarizeLambdaHandler(_mockOpenAiService.Object);

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(200, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Important Word 1, Important Word 2", (string)responseBody.importantWords);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnNotFound_WhenInvalidPath()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/invalid-path", "Some text.");

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(404, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Not Found", (string)responseBody);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnBadRequest_WhenTextIsMissing()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/summary");

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(400, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Text parameter is missing", (string)responseBody);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnInternalServerError_OnException()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/summary", "Some text.");
            var mockContext = new Mock<ILambdaContext>();
            var mockLogger = new Mock<ILambdaLogger>();
            mockLogger.Setup(l => l.LogLine(It.IsAny<string>())).Throws(new Exception("Logging error"));
            mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);

            var mockService = new Mock<IOpenAiService>();
            mockService.Setup(s => s.CallOpenAiAsync(It.IsAny<string>(), It.IsAny<ILambdaContext>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service error"));

            _handler = new SummarizeLambdaHandler(mockService.Object);

            // Act
            var response = await _handler.HandleAsync(request, mockContext.Object);

            // Assert
            Assert.Equal(500, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Internal Server Error", (string)responseBody.message);
        }

        [Fact]
        public async Task HandleSummaryAsync_ShouldReturnBadRequest_WhenServiceThrowsException()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/summary", "Some text.");
            _mockOpenAiService
                .Setup(service => service.CallOpenAiAsync(It.IsAny<string>(), It.IsAny<ILambdaContext>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service exception"));

            _handler = new SummarizeLambdaHandler(_mockOpenAiService.Object);

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(500, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Internal Server Error", (string)responseBody.message);
        }

        [Fact]
        public async Task HandleSummaryAsync_ShouldReturnBadRequest_WhenServiceReturnsEmptyResponse()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/summary", "Some text.");
            _mockOpenAiService
                .Setup(service => service.CallOpenAiAsync(It.IsAny<string>(), It.IsAny<ILambdaContext>(), It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            _handler = new SummarizeLambdaHandler(_mockOpenAiService.Object);

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(400, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Empty response from service", (string)responseBody);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnBadRequest_WhenInvalidJsonInRequest()
        {
            // Arrange
            var request = new APIGatewayProxyRequest
            {
                HttpMethod = "POST",
                Path = "/summary",
                Body = "Invalid JSON"
            };

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(400, response.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Invalid JSON format", (string)responseBody);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnBadRequest_WhenInvalidHttpMethod()
        {
            // Arrange
            var request = CreateApiGatewayRequest("/summary", "Some text.");
            request.HttpMethod = "GET"; // Invalid method for this endpoint

            // Act
            var response = await _handler.HandleAsync(request, CreateMockLambdaContext().Object);

            // Assert
            Assert.Equal(405, response.StatusCode); // Method Not Allowed
            var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);
            Assert.Equal("Method Not Allowed", (string)responseBody);
        }

        private APIGatewayProxyRequest CreateApiGatewayRequest(string path, string text = null)
        {
            return new APIGatewayProxyRequest
            {
                HttpMethod = "POST",
                Path = path,
                Body = text != null ? JsonConvert.SerializeObject(new { text }) : JsonConvert.SerializeObject(new { })
            };
        }
    }
}
