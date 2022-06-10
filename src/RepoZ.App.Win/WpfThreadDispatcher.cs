namespace RepoZ.App.Win;

using System;
using RepoZ.Api.Common;
using System.Windows;
using RepoM.Api.Common;

internal class WpfThreadDispatcher : IThreadDispatcher
{
    public void Invoke(Action act)
    {
        Application.Current.Dispatcher.Invoke(act);
    }
}