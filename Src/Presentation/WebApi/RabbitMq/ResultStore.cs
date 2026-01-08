using System.Collections.Concurrent;

namespace WebApi.RabbitMq
{
    public class ResultStore
    {
        private readonly ConcurrentDictionary<string, List<ProcessResult>> _dict = new();

        public void EnsureRequest(string requestId) =>
            _dict.TryAdd(requestId, new());

        public void Add(string requestId, ProcessResult result)
        {
            if (_dict.TryGetValue(requestId, out var list))
                lock (list) list.Add(result);
        }

        public IReadOnlyList<ProcessResult>? Get(string requestId) =>
            _dict.TryGetValue(requestId, out var list) ? list : null;
    }
}
