namespace RepoM.ActionMenu.Interface.ActionMenuFactory;

using System.IO.Abstractions;
using RepoM.Core.Plugin.Repository;

public interface IMenuContext
{
    IRepository Repository { get; }

    IFileSystem FileSystem { get; }
}