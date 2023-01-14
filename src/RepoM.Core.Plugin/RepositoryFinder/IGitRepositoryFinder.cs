namespace RepoM.Core.Plugin.RepositoryFinder;

using System;
using System.Collections.Generic;

public interface IGitRepositoryFinder
{
    List<string> Find(string root, Action<string> onFoundAction);
}