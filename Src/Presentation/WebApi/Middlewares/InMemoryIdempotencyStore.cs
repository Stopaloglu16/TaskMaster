using System.Collections.Concurrent;

namespace WebApi.Middlewares
{
    public class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly ConcurrentDictionary<string, IdempotentResponse> _store = new();

        public Task<IdempotentResponse?> GetAsync(string key)
            => Task.FromResult(_store.TryGetValue(key, out var value) ? value : null);

        public Task SetAsync(string key, IdempotentResponse response, TimeSpan ttl)
        {
            _store[key] = response;
            return Task.CompletedTask;
        }
    }
}
