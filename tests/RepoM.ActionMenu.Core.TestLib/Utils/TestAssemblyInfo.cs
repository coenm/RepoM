namespace RepoM.ActionMenu.Core.TestLib.Utils;

using System.Reflection;
using System;
using VerifyTests;

internal readonly struct TestAssemblyInfo
{
    /// <exception cref="ArgumentNullException">Thrown when argument is <c>null</c>.</exception>
    public TestAssemblyInfo(Assembly assembly)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        ProjectDirectory = AttributeReader.GetProjectDirectory(assembly);
        AttributeReader.TryGetSolutionDirectory(assembly, out var solutionDirectory);
        SolutionDirectory = solutionDirectory;
    }

    /// <summary>
    /// Assembly `containing` the TestFiles. This assembly should contain a ProjectReference to the EasyTestFile package.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// The project directory for the given <see cref="Assembly"/> on compile time.
    /// </summary>
    public string ProjectDirectory { get; }

    /// <summary>
    /// The solution directory for the given <see cref="Assembly"/> on compile time. Can be <c>null</c>.
    /// </summary>
    public string? SolutionDirectory { get; }
}