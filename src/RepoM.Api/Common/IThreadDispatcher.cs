namespace RepoM.Api.Common;

using System;

public interface IThreadDispatcher
{
    void Invoke(Action act);
}