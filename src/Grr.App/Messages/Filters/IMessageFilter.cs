namespace Grr.App.Messages.Filters;

public interface IMessageFilter
{
    void Filter(RepositoryFilterOptions filter);
}