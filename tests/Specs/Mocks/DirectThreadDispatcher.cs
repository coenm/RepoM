namespace Specs.Mocks;

using System;
using RepoM.Api.Common;
using RepoZ.Api.Common;

internal class DirectThreadDispatcher : IThreadDispatcher
{
    public void Invoke(Action act)
    {
        act();
    }
}