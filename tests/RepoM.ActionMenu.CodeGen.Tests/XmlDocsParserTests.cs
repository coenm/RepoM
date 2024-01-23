namespace RepoM.ActionMenu.CodeGen.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Misc;
using RepoM.ActionMenu.CodeGen.Models;
using VerifyXunit;
using Xunit;

public class XmlDocsParserTests
{
    [Fact]
    public async Task ExtractDocumentation_Should_When()
    {
        // arrange
        var files = new Dictionary<string, string>();
        var result = new MemberDescriptor();
        var xml = """
                  <member name="M:RepoM.ActionMenu.Core.Model.Functions.FileFunctions.FindFiles(RepoM.ActionMenu.Interface.ActionMenuFactory.IMenuContext,Scriban.Parsing.SourceSpan,System.String,System.String)">
                      <summary>
                      Find files in a given directory based on the search pattern. Resulting filenames are absolute path based.
                      </summary>
                      <param name="rootPath">The root folder.</param>
                      <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesn't support regular expressions.</param>
                      <returns>Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern.</returns>
                      <example>
                      Locate all solution files in the given directory.
                      <code>
                      find_files 'C:\Users\coenm\RepoM' '*.sln'
                      # find_files('C:\Users\coenm\RepoM','*.sln')
                      </code>
                      <code>
                      ["C:\Users\coenm\RepoM\src\RepoM.sln"]
                      </code>
                      </example>
                  </member>
                  """;
        
        // act
        XmlDocsParser.ExtractDocumentation(xml, A.Fake<ISymbol>(), result, files);

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task ExtractDocumentation2()
    {
        // arrange
        var files = new Dictionary<string, string>()
            {
                { "filename.yaml", "DUMMY CONTENT" },
            };
        var result = new MemberDescriptor();
        var xml = """
                  <member name="M:RepoM.ActionMenu.Core.Model.Functions.FileFunctions.FindFiles(RepoM.ActionMenu.Interface.ActionMenuFactory.IMenuContext,Scriban.Parsing.SourceSpan,System.String,System.String)">
                      <summary>
                      Find files in a given directory based on the search pattern. Resulting filenames are absolute path based.
                      </summary>
                      <param name="rootPath">The root folder.</param>
                      <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesn't support regular expressions.</param>
                      <returns>Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern.</returns>
                      <example>
                      Locate all solution files in the given directory.
                      <code>
                      find_files 'C:\Users\coenm\RepoM' '*.sln'
                      # find_files('C:\Users\coenm\RepoM','*.sln')
                      </code>
                      <code>
                      ["C:\Users\coenm\RepoM\src\RepoM.sln"]
                      </code>
                      
                        <usage/>
                        test / para
                        <code>
                        avc
                        </code>
                        
                        <result/>
               
                        test / para
                        
                        <repository-action-sample/>

                        <code type='yaml'>abc</code>
                        <code-file language='yaml' filename='filename.yaml'/>
                        <md-snippet>name1</md-snippet>
                        <md-include>name2</md-include>
                        
                      </example>
                  </member>
                  """;

        // act
        XmlDocsParser.ExtractDocumentation(xml, A.Fake<ISymbol>(), result, files);

        // assert
        await Verifier.Verify(result);
    }
}