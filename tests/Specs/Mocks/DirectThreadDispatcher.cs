namespace Specs.Mocks;

using System;
using RepoM.Api.Common;

internal class DirectThreadDispatcher : IThreadDispatcher
{
    public void Invoke(Action act)
    {
        act();
    }
}