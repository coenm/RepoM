namespace RepoM.ActionMenu.Core.Tests.Plugin
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using System.Threading.Tasks;
    using FakeItEasy;
    using RepoM.ActionMenu.Interface.UserInterface;
    using RepoM.Core.Plugin.Repository;
    using RepoM.ActionMenu.Core.Tests;
    using SimpleInjector;
    using VerifyXunit;
    using Xunit;
    using RepoM.ActionMenu.Interface.Scriban;

    [UsesVerify]
    public class PluginTests
    {
        private readonly IRepository _repository = new DummyRepository();
        private readonly Container _container;
     
        private const string YAML =
            """
            context:
              
            - type: render-variable@1
              name: my-text
              value: text `{{ name }}` text2
              enabled: 1 == 1

            tags:
            - tag: private
           
            action-menu:
           
            - type: foreach@1
              enumerable: dummy.configurations
              variable: item
              actions:
              - type: just-text@1
                text: 'key {{ item.key }}'
              - type: foreach@1
                enumerable: item.database_names
                variable: dbname
                actions:
                - type: just-text@1
                  text: 'dbname {{ dbname }}'  
         
            """;

        public PluginTests()
        {
            _container = new Container();
            Bootstrapper.RegisterServices(_container);

            var fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>
                    {
                        { "C:\\RepositoryActionsV2.yaml", new MockFileData(YAML, Encoding.UTF8) },
                });
            _container.RegisterInstance<IFileSystem>(fileSystem);

            IDummyService dummyService = A.Fake<IDummyService>();
            _container.RegisterInstance(dummyService);
            _container.Collection.Append<ITemplateContextRegistration, DummyVariablesProvider>(Lifestyle.Transient);


            var dummyValues = new DummyConfig[]
                {
                    new()
                        {
                            Host = "h1",
                            DatabaseNames = new [] {"x1", "x2", },
                            Key = "k1",
                        },
                    new()
                        {
                            Host = "h2",
                            DatabaseNames = new [] {"x21", "x22", },
                            Key = "k2",
                        },
                };

            A.CallTo(() => dummyService.GetValues()).Returns(dummyValues.ToImmutableArray());

            _container.Options.EnableAutoVerification = false;
        }

        [Fact]
        public async Task UseFactory()
        {
            // arrange
            IUserInterfaceActionMenuFactory sut = Bootstrapper.GetUserInterfaceActionMenu(_container);

            // act
            IEnumerable<UserInterfaceRepositoryActionBase> result = await sut.CreateMenuAsync(_repository, "C:\\RepositoryActionsV2.yaml");

            // assert
            await Verifier.Verify(result);
        }
    }
}