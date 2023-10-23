namespace RepoM.ActionMenu.Core.Tests
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using System.Threading.Tasks;
    using RepoM.ActionMenu.Core;
    using RepoM.ActionMenu.Core.Model;
    using RepoM.ActionMenu.Interface.UserInterface;
    using RepoM.Core.Plugin.Repository;
    using SimpleInjector;
    using VerifyXunit;
    using Xunit;

    [UsesVerify]
    public class ConvertTests
    {
        private readonly IRepository _repository = new DummyRepository();
        private readonly Container _container;

        private const string YAML =
            """
            context:
            - type: evaluate-script@1
              content: |-
                func remotes_contain_inner(remotes, url_part)
                  urls = remotes | array.map "url";
                  filtered = array.filter(urls, do
                    ret string.contains($0, url_part);
                  end);
                  ret array.size(filtered) > 0;
                end

                func remotes_contain(url_part)
                  ret remotes_contain_inner(repository.remotes, url_part)
                end
                
                func get_remote_origin()
                  remotes = repository.remotes;
                  filtered = array.filter(remotes, do 
                    remote = $0;
                    ret remote.key == "origin"
                  end)
                  ret array.first(filtered);
                end
            
                func get_remote_origin_name()
                  remote = get_remote_origin();
                  ret remote.name;
                end
                
                func safe_path_contains(path)
                  ret repository.safe_path | string.contains path
                end    
            
                remote_name_origin = get_remote_origin_name();
                is_bdo_nl_repository = remotes_contain("BDO-NL");
                is_github_repository = remotes_contain("github.com");
                
            - BdoRepoRoot: C:\Projects\Bdo\git\
            - BdoDevOpsRootUrl: https://dev.azure.com/tfs-bdonl/BDO-NL/
            - MonitoringExe: C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestRunner.Application\\bin\\Release\\net6.0-windows\\TestRunner.Application.exe
            - HeidiSqlExe: C:\\StandAloneProgramFiles\\HeidiSQL_12.3_64_Portable\\heidisql.exe
            # - SsmsExe: 'C:\\Program Files (x86)\\Microsoft SQL Server Management Studio 18\\Common7\\IDE\\Ssms.exe'
            - RootPathRepoM: C:\\Users\\Munckhof CJJ\\OneDrive - BDO\\BDO\\RepoM\\
            
            # Specific var files
            #- type: render-variable@1
              #name: RepoDocsDirectory
              #value: 'G:\\My Drive\\RepoDocs\\github.com\\{{repository.remote.origin.name}}' #todo
              # enabled: 'array.size repository.remotes > 0 && repository.remotes | array.map "url" | string.contains "github.com"'
              # enabled: repository.remotes.urls | string.contains "Projects/Github" '{StringContains({repository.RemoteUrls}, "github.com")}'  #todo
            
            tags:
            
            - tag: github-conditional
              when: safe_path_contains "Projects/Github"
            
            action-menu:
            - type: just-text@1
              text: 'Non existing env var {{ env.abcdefCOEN }}' 
              active: true
            
            - type: just-text@1
              text: '{{  remotes_contain("github.com") }}  {{  remote_name_origin }}' 
              # 'repository.pwd2 {{ repository.pwd }}'  
              active: is_github_repository
              
            - type: just-text@1
              text: '{{ repository.safe_path }} XX {{ safe_path_contains "C:/Projects" }}' 
              active: safe_path_contains "C:/Projects/" 
              
            """;

        public ConvertTests()
        {
            _container = new Container();
            Bootstrapper.RegisterServices(_container);

            var fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>
                    {
                        { "C:\\RepositoryActionsV2.yaml", new MockFileData(YAML, Encoding.UTF8) },
                    });
            _container.RegisterInstance<IFileSystem>(fileSystem);
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