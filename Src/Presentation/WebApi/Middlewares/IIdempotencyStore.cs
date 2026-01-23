namespace WebApi.Middlewares
{
    public interface IIdempotencyStore
    {
        Task<IdempotentResponse?> GetAsync(string key);
        Task SetAsync(string key, IdempotentResponse response, TimeSpan ttl);
    }

    public record IdempotentResponse(int StatusCode, string Body);
}
