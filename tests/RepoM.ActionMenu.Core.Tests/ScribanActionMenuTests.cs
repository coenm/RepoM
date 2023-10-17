namespace RepoM.ActionMenu.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using System.Threading.Tasks;
    using RepoM.ActionMenu.Core.PublicApi;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.BrowseRepository;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Folder;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Fetch;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Pull;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Push;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Ignore;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Pin;
    using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Separator;
    using RepoM.ActionMenu.Core.Yaml.Serialization;
    using RepoM.ActionMenu.Interface.Scriban;
    using RepoM.ActionMenu.Interface.UserInterface;
    using RepoM.ActionMenu.Interface.YamlModel;
    using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
    using RepoM.Core.Plugin.Repository;
    using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
    using VerifyXunit;
    using Xunit;

    [UsesVerify]
    public class ScribanActionMenuTests
    {
        private readonly IRepository _repository = new DummyRepository();
        private readonly Factory _sut;
        private readonly List<IKeyTypeRegistration<IMenuAction>> _registrations;

        private const string FILE1_ENV =
            """
            # just a comment
            ABC=my abc
            
            # comment 2
            DEF=my def 12
            """;

        private const string FILE2_ENV =
            """
            DEF=my def 12-second!
            GHI=GHI GHI GHI-second
            """;

        private const string SUB =
            """
            context:
            # - type: set-variable@1
            #   name: name
            #   value: subcoen
            
            - type: evaluate-script@1
              content: |-
                name = name + 'sub coen'
                ax = 'beerx';
                bx = 'winex';
            """;

        private const string YAML =
            """
            context:
            - type: evaluate-script@1
              content: |-
                func translate(input)
                  ret 'translate says:' + input;
                end
            
            - name: coenm
            - name1: coenm1
              
            - type: set-variable@1
              name: devopsEnvironments
              value: 
              - name: Develop
                url: '-d.bdodt.nl'
              - name: Test
                url: '-t.bdodt.nl'
              - name: Acceptation
                url: '-a.bdo.nl'
              - name: Production
                url: '.bdo.nl'  
                
            - type: evaluate-script@1
              content: |-
                a = 'beer';
                b = 'wine';
                name = name + ' drinks a lot of ' + a + ' and ' + b;
                
                now2 = date.now;
                
                my_age = 39;
                
                func sub1
                   ret $0 - $1
                end
                
                func sub2(x,y)
                   ret x - y + 10
                end
                
                func sonar_url(project_id)
                  ret 'https://sonarcloud.io/project/overview?id='  + project_id;
                end
                
                dummy_calc = sub2(19, 3);
                
            - type: load-file@1
              filename: 'C:\file1.env'
                  
            - type: render-variable@1
              name: my-text
              value: text `{{ name }}` text2
              enabled: 1 == 1
            
            tags:
            - tag: private
              when: 1 == 1

            - tag: github
              when: 1 == 2
              
            - tag: github
              when: 1 == 2  || true

            - tag: work
            
            action-menu:
            - type: just-text@1
              text: 'repository.pwd2 {{ repository.pwd }}'    
              
            - type: foreach@1
              enumerable: 'file.find_files("c:\\", "*.env")'
              variable: f
              actions:
              - type: just-text@1
                text: 'file name {{ f }}'
                
            - type: foreach@1
              enumerable: devopsEnvironments
              variable: environment
              actions:
              - type: just-text@1
                text: 'env name {{ environment.name }} env(DEF):`{{ env.DEF }}`'
            
            - type: associate-file@1
              name: Open {{ name }} in {{ 'visual' | string.upcase }} Studio Code {{ sub1(10,3) }}
              extension: .cs
              active: 1 == 1
            
            - type: just-text@1
              text: Text {{ name }} in {{ 'visual' | string.upcase }} Studio Code {{ sub1(10,3) }} {{ link }} env(DEF) {{ env.DEF }}
              active: 1 <= 3
              context:
              - type: evaluate-variable@1
                name: link
                value: 'sonar_url "sf23-2"'
              - type: load-file@1
                filename: 'C:\SubV2.yaml'
              - type: load-file@1
                filename: 'C:\file2.env'
            - type: folder@1
              name: my-folder
              is-deferred: 1 >= 2 || false
              context:
              - type: set-variable@1
                name: name
                value: coenm23
              actions:
              - type: just-text@1
                text: Text {{ name }} in {{ 'visual' | string.upcase }} Studio Code {{ sub1(10,3) }} {{repository.is_starred}}
                active: 1 <= 3
                context:
                - type: set-variable@1
                  name: name2
                  value: namenamename
                  
            - type: folder@1
              name: git
              is-deferred: false
              context:
              actions:
              - type: git-checkout@1
              - type: git-checkout@1
                name: CheckOut!
            """;

        public ScribanActionMenuTests()
        {
            _registrations = new List<IKeyTypeRegistration<IMenuAction>>
                {
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionAssociateFileV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionBrowseRepositoryV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionCommandV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionFolderV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionForEachV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionGitCheckoutV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionGitFetchV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionGitPullV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionGitPushV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionIgnoreV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionJustTextV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionPinV1>(),
                    ContainerExtensions.CreateRegistrationObject<RepositoryActionSeparatorV1>(),
                };

            List<IActionToRepositoryActionMapper> mappers = new()
                {
                    new RepositoryActionAssociateFileV1Mapper(),
                    new RepositoryActionBrowseRepositoryV1Mapper(),
                    new RepositoryActionCommandV1Mapper(),
                    new RepositoryActionFolderV1Mapper(),
                    new RepositoryActionForEachV1Mapper(),
                    new RepositoryActionGitCheckoutV1Mapper(),
                    new RepositoryActionGitFetchV1Mapper(),
                    new RepositoryActionGitPullV1Mapper(),
                    new RepositoryActionGitPushV1Mapper(),
                    new RepositoryActionIgnoreV1Mapper(),
                    new RepositoryActionJustTextV1Mapper(),
                    new RepositoryActionPinV1Mapper(),
                    new RepositoryActionSeparatorV1Mapper(),
                };

            var fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>
                    {
                        { "C:\\RepositoryActionsV2.yaml", new MockFileData(YAML, Encoding.UTF8) },
                        { "C:\\SubV2.yaml", new MockFileData(SUB, Encoding.UTF8) },
                        { "C:\\file1.env", new MockFileData(FILE1_ENV, Encoding.UTF8) },
                        { "C:\\file2.env", new MockFileData(FILE2_ENV, Encoding.UTF8) },
                    });

            _sut = new Factory(fileSystem, Array.Empty<ITemplateContextRegistration>(), _registrations, mappers);
        }

        [Fact]
        public async Task UseFactory()
        {
            // arrange
            IUserInterfaceActionMenuFactory factory = _sut.Create();

            // act
            IEnumerable<UserInterfaceRepositoryActionBase> result = await factory.CreateMenuAsync(_repository, "C:\\RepositoryActionsV2.yaml");

            // assert
            await Verifier.Verify(result);
        }

        [Fact]
        public async Task GetTags()
        {
            // arrange
            IUserInterfaceActionMenuFactory factory = _sut.Create();

            // act
            IEnumerable<string> result = await factory.GetTagsAsync(_repository, "C:\\RepositoryActionsV2.yaml");

            // assert
            await Verifier.Verify(result);
        }
        
        [Fact]
        public async Task Serialize()
        {
            // arrange
            var deserializer = new ActionMenuDeserializer(_registrations);

            // act
            var result = deserializer.Serialize(deserializer.DeserializeRoot(YAML));

            // assert
            await Verifier.Verify(result);
        }
    }
}