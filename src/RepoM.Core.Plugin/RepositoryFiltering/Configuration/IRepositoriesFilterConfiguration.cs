namespace RepoM.Core.Plugin.RepositoryFiltering.Configuration;

public class RepositoryFilterConfiguration
{
    public string Name { get; set; }    

    public string Description { get; set; }

    public QueryConfiguration AlwaysVisible { get; set; }

    public QueryConfiguration Filter { get; set; }
}

public class QueryConfiguration
{
    public string Kind { get; set; }

    public string Query { get; set; }
}