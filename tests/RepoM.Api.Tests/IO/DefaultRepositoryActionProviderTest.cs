namespace RepoM.Api.Common.Tests.IO;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using EasyTestFileXunit;
using ExpressionStringEvaluator.Methods.BooleanToBoolean;
using ExpressionStringEvaluator.Methods.Flow;
using ExpressionStringEvaluator.Methods.StringToBoolean;
using ExpressionStringEvaluator.Methods.StringToInt;
using ExpressionStringEvaluator.Methods.StringToString;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using ExpressionStringEvaluator.VariableProviders;
using FakeItEasy;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.IO.VariableProviders;
using VerifyXunit;

[UsesEasyTestFile]
[UsesVerify]
public class DefaultRepositoryActionProviderTest
{
    private readonly IAppDataPathProvider _appDataPathProvider = A.Fake<IAppDataPathProvider>();
    private readonly IRepositoryWriter _repositoryWriter = A.Fake<IRepositoryWriter>();
    private readonly IRepositoryMonitor _repositoryMonitor = A.Fake<IRepositoryMonitor>();
    private readonly ITranslationService _translationService = A.Fake<ITranslationService>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly List<IVariableProvider> _providers;
    private readonly List<IMethod> _methods;

    public DefaultRepositoryActionProviderTest()
    {
        A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns("GetAppDataPath");
        A.CallTo(() => _appDataPathProvider.GetAppResourcesPath()).Returns("GetAppResourcesPath");

        A.CallTo(() => _translationService.Translate(A<string>._)).ReturnsLazily(call => (call.Arguments[0] as string)!);

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

        _providers = new List<IVariableProvider>
            {
                new DateTimeNowVariableProvider(dateTimeNowVariableProviderOptions),
                new DateTimeTimeVariableProvider(dateTimeTimeVariableProviderOptions),
                new DateTimeDateVariableProvider(dateTimeDateVariableProviderOptions),
                new EmptyVariableProvider(),
                new CustomEnvironmentVariableVariableProvider(),
                new RepoMVariableProvider(),
                new RepositoryVariableProvider(),
                new SlashVariableProvider(),
                new BackslashVariableProvider(),
            };

        _methods = new List<IMethod>
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
    }
}