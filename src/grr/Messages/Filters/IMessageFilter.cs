namespace Grr.Messages.Filters;

public interface IMessageFilter
{
    void Filter(RepositoryFilterOptions filter);
}