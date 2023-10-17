namespace RepoM.ActionMenu.Core.Tests
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using System.Threading.Tasks;
    using RepoM.ActionMenu.Core.Model;
    using RepoM.ActionMenu.Core.PublicApi;
    using RepoM.ActionMenu.Interface.UserInterface;
    using RepoM.Core.Plugin.Repository;
    using SimpleInjector;
    using VerifyXunit;
    using Xunit;

    [UsesVerify]
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
            _container = new Container();
            Bootstrapper.RegisterServices(_container);

            var fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>
                    {
                        { "C:\\RepositoryActionsV2.yaml", new MockFileData(YAML, Encoding.UTF8) },
                        { "C:\\SubV2.yaml", new MockFileData(SUB, Encoding.UTF8) },
                        { "C:\\file1.env", new MockFileData(FILE1_ENV, Encoding.UTF8) },
                        { "C:\\file2.env", new MockFileData(FILE2_ENV, Encoding.UTF8) },
                    });
            _container.RegisterInstance<IFileSystem>(fileSystem);
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

        [Fact]
        public async Task GetTags()
        {
            // arrange
            IUserInterfaceActionMenuFactory sut = Bootstrapper.GetUserInterfaceActionMenu(_container);

            // act
            IEnumerable<string> result = await sut.GetTagsAsync(_repository, "C:\\RepositoryActionsV2.yaml");

            // assert
            await Verifier.Verify(result);
        }
        
        [Fact]
        public async Task Serialize()
        {
            // arrange
            IActionMenuDeserializer deserializer = _container.GetInstance<IActionMenuDeserializer>();

            // act
            var result = deserializer.Serialize(deserializer.DeserializeRoot(YAML));

            // assert
            await Verifier.Verify(result);
        }
    }
}