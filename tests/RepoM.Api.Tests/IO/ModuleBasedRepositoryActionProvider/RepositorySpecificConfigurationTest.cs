namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.Methods.BooleanToBoolean;
using ExpressionStringEvaluator.Methods.Flow;
using ExpressionStringEvaluator.Methods.StringToBoolean;
using ExpressionStringEvaluator.Methods.StringToInt;
using ExpressionStringEvaluator.Methods.StringToString;
using ExpressionStringEvaluator.VariableProviders;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Api.IO.VariableProviders;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Expressions;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class RepositorySpecificConfigurationTest
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly MockFileSystem _fileSystem;
    private readonly YamlDynamicRepositoryActionDeserializer _yamlAppSettingsDeserializer;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly string _tempPath;
    private readonly IRepositoryExpressionEvaluator _repositoryExpressionEvaluator;
    private readonly ActionMapperComposition _actionMapperComposition;
    private readonly ITranslationService _translationService;

    public RepositorySpecificConfigurationTest()
    {
        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
        _verifySettings.IgnoreMember(nameof(Repository));

        _tempPath = Path.GetTempPath();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), "C:\\");

        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => _appDataPathProvider.AppDataPath).Returns(_tempPath);

        JsonDynamicRepositoryActionDeserializer jsonAppSettingsDeserializer = DynamicRepositoryActionDeserializerFactory.Create();
        _yamlAppSettingsDeserializer = new YamlDynamicRepositoryActionDeserializer(jsonAppSettingsDeserializer);

        var dateTimeTimeVariableProviderOptions = new DateTimeVariableProviderOptions()
        {
            DateTimeProvider = () => DateTime.Now,
        };

        var dateTimeNowVariableProviderOptions = new DateTimeNowVariableProviderOptions()
        {
            DateTimeProvider = () => DateTime.Now,
        };

        var dateTimeDateVariableProviderOptions = new DateTimeDateVariableProviderOptions()
        {
            DateTimeProvider = () => DateTime.Now,
        };

        var providers = new List<IVariableProvider>
            {
                new DateTimeNowVariableProvider(dateTimeNowVariableProviderOptions),
                new DateTimeTimeVariableProvider(dateTimeTimeVariableProviderOptions),
                new DateTimeDateVariableProvider(dateTimeDateVariableProviderOptions),
                new EmptyVariableProvider(),
                new VariableProviderAdapter(new Core.Plugin.VariableProviders.IVariableProvider[]
                    {
                        new CustomEnvironmentVariableVariableProvider(),
                        new RepoMVariableProvider(),
                        new RepositoryVariableProvider(),
                    }),
                new SlashVariableProvider(),
                new BackslashVariableProvider(),
            };

        var methods = new List<IMethod>
            {
                new StringTrimEndStringMethod(),
                new StringTrimStartStringMethod(),
                new StringTrimStringMethod(),
                new StringContainsStringMethod(),
                new StringLowerStringMethod(),
                new StringUpperStringMethod(),
                new UrlEncodeStringMethod(),
                new UrlDecodeStringMethod(),
                new StringEqualsStringMethod(),
                new AndBooleanMethod(),
                new OrBooleanMethod(),
                new StringIsNullOrEmptyBooleanMethod(),
                new FileExistsBooleanMethod(),
                new NotBooleanMethod(),
                new StringLengthMethod(),
                new IfThenElseMethod(),
                new IfThenMethod(),
                new InMethod(),
                new StringReplaceMethod(),
                new SubstringMethod(),
            };

        _repositoryExpressionEvaluator = new RepositoryExpressionEvaluator(providers, methods);

        _translationService = A.Fake<ITranslationService>();
        A.CallTo(() => _translationService.Translate(A<string>._)).ReturnsLazily(call => (call.Arguments[0] as string)!);
        IRepositoryWriter repositoryWriter = A.Fake<IRepositoryWriter>();
        IRepositoryMonitor repositoryMonitor = A.Fake<IRepositoryMonitor>();

        _actionMapperComposition = ActionMapperCompositionFactory.Create(
            _repositoryExpressionEvaluator,
            _translationService,
            _fileSystem,
            repositoryWriter,
            repositoryMonitor);
    }

    [Fact]
    public async Task Create_ShouldRespectMultiSelectRepos()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActionsMultiSelect");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        RepositorySpecificConfiguration sut = CreateSut();

        // act
        IEnumerable<RepositoryActionBase> result = sut.CreateActions(new Repository("path1"), new Repository("path2"));

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Create_ShouldNotCareAboutMultiSelectRepos_WhenSingleRepo()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActionsMultiSelect");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        RepositorySpecificConfiguration sut = CreateSut();

        // act
        IEnumerable<RepositoryActionBase> result = sut.CreateActions(new Repository("path1"));

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Create_ShouldOnlyProcessActiveItems()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        RepositorySpecificConfiguration sut = CreateSut();

        // act
        IEnumerable<RepositoryActionBase> result = sut.CreateActions(new Repository("path1"));

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_ShouldProcessSeparator1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActionsWithSeparator1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _fileSystem.AddFile(Path.Combine(_tempPath, RepositoryConfigurationReader.FILENAME), new MockFileData(content, Encoding.UTF8));
        RepositorySpecificConfiguration sut = CreateSut();

        // act
        IEnumerable<RepositoryActionBase> result = sut.CreateActions(new Repository("path1"));

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    private RepositorySpecificConfiguration CreateSut()
    {
        return new RepositorySpecificConfiguration(
            _fileSystem,
            _repositoryExpressionEvaluator,
            _actionMapperComposition,
            _translationService,
            new RepositoryConfigurationReader(
                _appDataPathProvider,
                _fileSystem,
                _yamlAppSettingsDeserializer,
                _repositoryExpressionEvaluator,
                NullLogger.Instance));
    }
}