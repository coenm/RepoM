namespace RepoM.Plugin.Heidi.Tests.Internal;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Plugin.Heidi.Internal;
using Xunit;

public class HeidiConfigurationServiceTests
{
    private ILogger _logger;
    private IFileSystem _fileSystem;

    public HeidiConfigurationServiceTests()
    {
        _logger = NullLogger.Instance;
        _fileSystem = new MockFileSystem();
    }

    [Fact]
    public void Abc()
    {
        // arrange

        // act
        var sut = new HeidiConfigurationService(
            _logger,
            _fileSystem,
            new HeidiPortableConfigReader(_fileSystem, _logger),
            new HeidiSettings());

        // assert
        // todo


    }
}