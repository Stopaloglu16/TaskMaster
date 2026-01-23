using System.Text;

namespace WebApi.Middlewares
{
    public class IdempotencyFilter : IEndpointFilter
    {
        private readonly IIdempotencyStore _store;

        public IdempotencyFilter(IIdempotencyStore store)
        {
            _store = store;
        }

        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var http = context.HttpContext;

            if (!http.Request.Headers.TryGetValue("Idempotency-Key", out var key))
            {
                return Results.BadRequest("Missing Idempotency-Key header");
            }

            var cacheKey = $"{http.Request.Path}:{key}";

            var cached = await _store.GetAsync(cacheKey);
            if (cached != null)
            {
                return Results.Content(
                    cached.Body,
                    statusCode: cached.StatusCode,
                    contentType: "application/json");
            }

            var result = await next(context);

            if (result is IResult iresult)
            {
                var responseBody = await SerializeResultAsync(iresult, http);
                await _store.SetAsync(
                    cacheKey,
                    new IdempotentResponse(http.Response.StatusCode, responseBody),
                    TimeSpan.FromMinutes(10));
            }

            return result;
        }

        private static async Task<string> SerializeResultAsync(IResult result, HttpContext http)
        {
            using var ms = new MemoryStream();
            var originalBody = http.Response.Body;
            http.Response.Body = ms;

            await result.ExecuteAsync(http);

            http.Response.Body = originalBody;
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

}
