namespace RepoM.Core.Repositories.Watching;

using System;
using System.Collections.Generic;
using RepoM.Core.Repositories.Model;

public interface IRepositoryWatcher : IDisposable
{
    IObservable<RepositoryChangeEvent> Watch(IEnumerable<string> paths);
}
