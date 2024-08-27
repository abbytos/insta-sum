using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace SummarizeLambdaHandler
{
    /// <summary>
    /// Defines the interface for interacting with the OpenAI service.
    /// </summary>
    public interface IOpenAiService
    {
        /// <summary>
        /// Asynchronously calls the OpenAI API with the provided text and prompt, and returns the generated summary.
        /// </summary>
        /// <param name="text">The text to be summarized by OpenAI.</param>
        /// <param name="context">The Lambda context for logging and runtime information.</param>
        /// <param name="prompt">The prompt to guide OpenAI's response.</param>
        /// <returns>A task representing the asynchronous operation, with the summarized text as the result.</returns>
        Task<string> CallOpenAiAsync(string text, ILambdaContext context, string prompt);
    }
}
