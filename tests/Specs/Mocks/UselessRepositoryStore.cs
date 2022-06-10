namespace Specs.Mocks;

using System.Collections.Generic;
using System;
using RepoM.Api.Git;

internal class UselessRepositoryStore : IRepositoryStore
{
    public IEnumerable<string> Get()
    {
        return Array.Empty<string>();
    }

    public void Set(IEnumerable<string> paths) { }
}