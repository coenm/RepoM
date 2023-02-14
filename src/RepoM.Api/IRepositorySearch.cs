namespace RepoM.Api;

using System;
using System.Collections.Generic;

[Obsolete("Refactoring")]
public interface IRepositorySearch
{
    IEnumerable<string> Search(string query);
}