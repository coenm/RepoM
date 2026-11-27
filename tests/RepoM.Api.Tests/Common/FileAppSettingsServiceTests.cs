namespace RepoM.Api.Tests.Common;

using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Common;
using Xunit;

public class FileAppSettingsServiceTests
{
    [Fact]
    public void ExistingConfiguration_ShouldDefaultAppearanceWithoutChangingExistingPreferences()
    {
        var files = new MockFileSystem();
        files.AddFile(@"C:\Settings\appsettings.json", new MockFileData("""{"SortKey":"existing","PruneOnFetch":true}"""));
        FileAppSettingsService settings = Create(files);

        settings.Theme.Should().Be(AppTheme.Dark);
        settings.EnableAnimations.Should().BeTrue();
        settings.ContextMenuLoadingIndicatorDelayMilliseconds.Should().Be(500);
        settings.SortKey.Should().Be("existing");
        settings.PruneOnFetch.Should().BeTrue();
    }

    [Theory]
    [InlineData("999")]
    [InlineData("\"999\"")]
    public void UnknownNumericTheme_ShouldUseDarkAndPreserveOtherSettings(string theme)
    {
        var files = new MockFileSystem();
        files.AddFile(@"C:\Settings\appsettings.json", new MockFileData($$"""{"Theme":{{theme}},"SortKey":"existing","PruneOnFetch":true}"""));
        var logger = A.Fake<ILogger>();
        FileAppSettingsService settings = Create(files, logger);

        settings.Theme.Should().Be(AppTheme.Dark);
        settings.SortKey.Should().Be("existing");
        settings.PruneOnFetch.Should().BeTrue();
        A.CallTo(logger)
            .Where(call => call.Method.Name == "Log" && call.Arguments.Get<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(AppTheme.Dark)]
    [InlineData(AppTheme.Light)]
    [InlineData(AppTheme.System)]
    public void Appearance_ShouldRoundTripAndPreserveOtherSettings(AppTheme theme)
    {
        var files = new MockFileSystem();
        FileAppSettingsService settings = Create(files);
        settings.SortKey = "existing";
        settings.Theme = theme;
        settings.EnableAnimations = false;

        FileAppSettingsService reloaded = Create(files);
        reloaded.Theme.Should().Be(theme);
        reloaded.EnableAnimations.Should().BeFalse();
        reloaded.SortKey.Should().Be("existing");
        files.File.ReadAllText(@"C:\Settings\appsettings.json").Should().Contain($"\"Theme\": \"{theme}\"");
    }

    private static FileAppSettingsService Create(MockFileSystem files, ILogger? logger = null)
    {
        var paths = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => paths.AppDataPath).Returns(@"C:\Settings");
        return new FileAppSettingsService(paths, files, logger ?? NullLogger.Instance);
    }
}
