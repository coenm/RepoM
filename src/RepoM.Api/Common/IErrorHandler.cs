namespace RepoM.Api.Common;

public interface IErrorHandler
{
    void Handle(string error);
}