namespace RepoM.App.Plugins;

using System;

public interface IModuleManager
{
    event EventHandler? Changed;
}