using Application.Common.Interfaces;

namespace WorkerServiceProcess;

/// <summary>
/// There is no signed-in user in the worker, but <c>ApplicationDbContext</c> stamps audit fields from
/// <see cref="ICurrentUserService"/> and skips them entirely when it is absent. Attributing the
/// saga's writes to a system identity keeps Created/LastModified populated.
/// </summary>
public sealed class SystemUserService : ICurrentUserService
{
    public string UserId => "system";

    public string UserName => "filejob-saga";
}
