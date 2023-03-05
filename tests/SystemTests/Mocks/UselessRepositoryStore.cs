namespace SystemTests.Mocks;

using System;
using System.Collections.Generic;
using RepoM.Api.Git;

internal class UselessRepositoryStore : IRepositoryStore
{
    public IEnumerable<string> Get()
    {
        return Array.Empty<string>();
    }

    public void Set(IEnumerable<string> paths) { }
}