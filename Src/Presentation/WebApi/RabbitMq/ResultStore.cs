using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using System.Collections.Concurrent;

namespace WebApi.RabbitMq
{
    public class ResultStore
    {
        private readonly ConcurrentDictionary<string, List<CreateTaskListResponse>> _dict = new();

        public void EnsureRequest(string requestId) =>
            _dict.TryAdd(requestId, new());

        public void Add(string requestId, IEnumerable<CreateTaskListResponse> results)
        {
            var list = _dict.GetOrAdd(requestId, _ => new());
            lock (list) list.AddRange(results);
        }

        public IReadOnlyList<CreateTaskListResponse>? Get(string requestId) =>
            _dict.TryGetValue(requestId, out var list) ? list : null;
    }
}
