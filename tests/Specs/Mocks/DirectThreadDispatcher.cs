namespace Specs.Mocks;

using System;
using RepoZ.Api.Common;

internal class DirectThreadDispatcher : IThreadDispatcher
{
    public void Invoke(Action act)
    {
        act();
    }
}