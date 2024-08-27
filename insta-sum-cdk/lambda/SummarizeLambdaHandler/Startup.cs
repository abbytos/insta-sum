using Microsoft.Extensions.DependencyInjection;

namespace SummarizeLambdaHandler
{
    /// <summary>
    /// The Startup class is responsible for configuring the services required by the application.
    /// It registers dependencies that are used by the Lambda handler.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures the services for dependency injection.
        /// This method is called by the runtime to add services to the container.
        /// </summary>
        /// <param name="services">The IServiceCollection to which services are added.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Register the OpenAiService as a transient service. 
            // This means a new instance will be created each time it is requested.
            services.AddTransient<IOpenAiService, OpenAiService>();

            // Register the SummarizeLambdaHandler as a singleton service. 
            // This means a single instance will be created and shared across the application.
            services.AddSingleton<SummarizeLambdaHandler>();
        }
    }
}
