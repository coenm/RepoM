namespace RepoM.ActionMenu.Core.Tests
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using RepoM.ActionMenu.Core;
    using RepoM.ActionMenu.Interface.UserInterface;
    using RepoM.Core.Plugin.Repository;
    using SimpleInjector;
    using VerifyXunit;
    using Xunit;

    public class ScribanActionMenuTests
    {
        private readonly IRepository _repository = new DummyRepository();
        private readonly Container _container;

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

        private const string TAGS =
            """
            context:
            - name: coenm
            - name1: coenm1
              
            - type: set-variable@1
              name: devopsEnvironments
              value: 
              - name: Develop
                url: '-d.github-dt.nl'
              - name: Test
                url: '-t.github-dt.nl'
              - name: Acceptation
                url: '-a.github.nl'
              - name: Production
                url: '.github.nl'  
                
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
            
            - tag: github-conditional
              when: repository.linux_path | string.contains "Projects/Github"
            """;

        private const string ACTION_MENU =
            """
            context:
            - name: coenm
            - name1: coenm1
              
            - type: set-variable@1
              name: devopsEnvironments
              value: 
              - name: Develop
                url: '-d.github-dt.nl'
              - name: Test
                url: '-t.github-dt.nl'
              - name: Acceptation
                url: '-a.github.nl'
              - name: Production
                url: '.github.nl'  
                
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
            
            action-menu:
            - type: just-text@1
              name: 'repository.pwd2 {{ repository.pwd }}'  
              
            - type: foreach@1
              enumerable: 'file.find_files("c:\\", "*.env")'
              variable: f
              actions:
              - type: just-text@1
                name: 'file name {{ f }}'
                
            - type: foreach@1
              enumerable: devopsEnvironments
              variable: environment
              actions:
              - type: just-text@1
                name: 'env name {{ environment.name }} env(DEF):`{{ env.DEF }}`'
            
            - type: just-text@1
              name: Text {{ name }} in {{ 'visual' | string.upcase }} Studio Code {{ sub1(10,3) }} {{ link }} env(DEF) {{ env.DEF }}
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
                name: Text {{ name }} in {{ 'visual' | string.upcase }} Studio Code {{ sub1(10,3) }} {{repository.is_starred}}
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
            _container = new Container();
            Bootstrapper.RegisterServices(_container);

            var fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>
                    {
                        { "C:\\RepositoryActionsV2.yaml", new MockFileData(ACTION_MENU, Encoding.UTF8) },
                        { "C:\\TagsV2.yaml", new MockFileData(TAGS, Encoding.UTF8) },
                        { "C:\\SubV2.yaml", new MockFileData(SUB, Encoding.UTF8) },
                        { "C:\\file1.env", new MockFileData(FILE1_ENV, Encoding.UTF8) },
                        { "C:\\file2.env", new MockFileData(FILE2_ENV, Encoding.UTF8) },
                    });
            _container.RegisterInstance<IFileSystem>(fileSystem);
            _container.RegisterInstance<ILogger>(NullLogger.Instance);
            _container.Options.EnableAutoVerification = false;
        }

        [Fact]
        public async Task UseFactory()
        {
            // arrange
            IUserInterfaceActionMenuFactory sut = Bootstrapper.GetUserInterfaceActionMenu(_container);

            // act
            IEnumerable<UserInterfaceRepositoryActionBase> result = await sut.CreateMenuListAsync(_repository, "C:\\RepositoryActionsV2.yaml");

            // assert
            await Verifier.Verify(result).ScrubMembersWithType<IRepository>();
        }

        [Fact]
        public async Task GetTags()
        {
            // arrange
            IUserInterfaceActionMenuFactory sut = Bootstrapper.GetUserInterfaceActionMenu(_container);

            // act
            IEnumerable<string> result = await sut.GetTagsAsync(_repository, "C:\\TagsV2.yaml");

            // assert
            await Verifier.Verify(result);
        }
    }
}