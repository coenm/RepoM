namespace RepoM.Core.Repositories.Scanning;

using System;
using System.Collections.Generic;
using System.Threading;

public interface IRepositoryScanner : IDisposable
{
    IObservable<string> Scan(IEnumerable<string> paths, CancellationToken ct = default);

    IObservable<bool> IsScanning { get; }
}
