namespace RepoM.ActionMenu.Interface.ActionMenuFactory;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.YamlModel;

public interface IScope : IDisposable
{
    void SetValue(string member, object? value, bool @readonly);

    void PushEnvironmentVariable(Dictionary<string, string> envVars);

    Task AddContextActionAsync(IContextAction contextItem);
}