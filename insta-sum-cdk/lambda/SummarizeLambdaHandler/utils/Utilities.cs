using Newtonsoft.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace SummarizeLambdaHandler
{
    public static class Utilities
    {
        public static string ExtractText(APIGatewayProxyRequest request)
        {
            var requestBody = JsonConvert.DeserializeObject<dynamic>(request.Body);
            return requestBody?.text?.ToString();
        }

        public static APIGatewayProxyResponse CreateResponse(int statusCode, object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = JsonConvert.SerializeObject(body)
            };
        }
    }
}
