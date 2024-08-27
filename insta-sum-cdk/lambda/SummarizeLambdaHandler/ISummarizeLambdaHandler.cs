using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace SummarizeLambdaHandler
{
    public interface ISummarizeLambdaHandler
    {
        Task<APIGatewayProxyResponse> HandleAsync(APIGatewayProxyRequest request, ILambdaContext context);
    }
}
