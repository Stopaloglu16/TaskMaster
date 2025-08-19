using Microsoft.Extensions.Http.Resilience;

namespace WebsiteApp.Handlers
{
    public static class ResilienceHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder RemoveAllResilienceHandlers(this IHttpClientBuilder builder)
        {
            builder.ConfigureAdditionalHttpMessageHandlers(static (handlers, _) =>
            {
                // Use type name string comparison to avoid direct reference to ResilienceHandler
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    var handlerType = handlers[i].GetType();
                    if (handlerType.FullName == "Microsoft.Extensions.Http.Resilience.ResilienceHandler")
                    {
                        handlers.RemoveAt(i);
                    }
                }
            });
            return builder;
        }
    }
}
