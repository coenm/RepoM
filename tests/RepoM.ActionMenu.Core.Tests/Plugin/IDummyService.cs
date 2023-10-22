namespace RepoM.ActionMenu.Core.Tests.Plugin;

using System.Collections.Immutable;

public interface IDummyService
{
    ImmutableArray<DummyConfig> GetValues();
}