namespace RepoM.Api;

using System.Collections.Generic;

public interface IRepositorySearch
{
    IEnumerable<string> Search(string query);
}