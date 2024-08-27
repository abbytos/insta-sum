using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System;

namespace SummarizeLambdaHandler
{
    public static class ErrorHandler
    {
        public static APIGatewayProxyResponse HandleException(Exception ex, ILambdaContext context)
        {
            LogException(context, ex);
            return CreateResponse(500, "Internal Server Error");
        }

        private static void LogException(ILambdaContext context, Exception ex)
        {
            try
            {
                context.Logger?.LogLine($"Exception occurred: {ex.Message}");
            }
            catch
            {
                // Logging failed, ignore the exception
            }
        }

        private static APIGatewayProxyResponse CreateResponse(int statusCode, string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = JsonConvert.SerializeObject(new { message })
            };
        }
    }
}