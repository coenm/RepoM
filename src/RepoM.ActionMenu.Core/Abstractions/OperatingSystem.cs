namespace RepoM.ActionMenu.Core.Abstractions;

using System;
using System.IO.Abstractions;

internal class OperatingSystem
{
    public OperatingSystem(IFileSystem fileSystem, IEnvironment environment)
    {
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <inheritdoc cref="IFileSystem"/>
    public IFileSystem FileSystem { get; }

    /// <inheritdoc cref="IEnvironment"/>
    public IEnvironment Environment { get; }
}