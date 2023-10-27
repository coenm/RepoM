namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.Internal;
using SimpleInjector;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class IntegrationActionMenuTests
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly Container _container;
    private readonly TestBootstrapper _bootstrapper;
    private readonly IRepository _repository;
    private readonly IAzureDevOpsPullRequestService _service;

    public IntegrationActionMenuTests()
    {
        _repository = new Repository("C:\\repositories\\work");
        var packageConfiguration = A.Dummy<IPackageConfiguration>();
        
        _bootstrapper = new TestBootstrapper(packageConfiguration);
        _bootstrapper.RegisterActionMenuLibrary();
        _bootstrapper.RegisterPlugin(new AzureDevOpsPackage());

        _appSettingsService = A.Fake<IAppSettingsService>();
        _bootstrapper.Container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>);
        _bootstrapper.Container.RegisterSingleton(A.Dummy<IActionToRepositoryActionMapper>);
        _bootstrapper.Container.RegisterInstance(_appSettingsService);

        _container = _bootstrapper.Container;

        _container.Options.AllowOverridingRegistrations = true;
        _container.Options.EnableAutoVerification = false;

        _service = A.Fake<IAzureDevOpsPullRequestService>();
        _container.RegisterInstance<IAzureDevOpsPullRequestService>(_service);

        A.CallTo(() => _service.GetPullRequests(_repository, "dummy_project_id", null!)).Returns(new List<PullRequest>()
            {
                new (Guid.Empty, "test pr", "https://azure-devops.test/pr/123"),
            });
    }

    [Fact]
    public void ContainerVerify()
    {
        _container.Verify();
    }

    private const string YAML =
           """
            context:
            - type: evaluate-script@1
              content: |-
                prs = azure_devops.get_pull_requests("dummy_project_id");
                first_pr = array.first prs;
            
            action-menu:
            - type: just-text@1
              name: 'pr count: {{ array.size prs }} url: "{{ first_pr.url }}"; name: "{{ first_pr.name  }}" '  
         
            """;

    [Fact]
    public async Task Context_GetPullRequests()
    {
        // arrange
        _bootstrapper.AddRootFile(YAML);
        IUserInterfaceActionMenuFactory factory = _bootstrapper.GetUserInterfaceActionMenu();

        // act
        var result = await factory.CreateMenuAsync(_repository, "C:\\RepositoriesV2.yaml");

        // assert
        await Verifier.Verify(result.Single());
    }
}