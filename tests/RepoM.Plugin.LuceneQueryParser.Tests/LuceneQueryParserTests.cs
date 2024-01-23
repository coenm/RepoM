namespace RepoM.Plugin.LuceneQueryParser.Tests;

using System.Threading.Tasks;
using Argon;
using FluentAssertions;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Plugin.LuceneQueryParser;
using VerifyTests;
using VerifyXunit;
using Xunit;

// https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
public class LuceneQueryParserTests
{
    private readonly LuceneQueryParser _sut;
    private readonly VerifySettings _settings;

    public LuceneQueryParserTests()
    {
        _sut = new LuceneQueryParser(NullLogger.Instance);

        _settings = new VerifySettings();
        _settings.AddExtraSettings(settings =>
            {
                settings.DefaultValueHandling = DefaultValueHandling.Include;
                settings.TypeNameHandling = TypeNameHandling.Auto;

            });
        _settings.DisableRequireUniquePrefix();
    }

    [Fact]
    public void Name_ShouldReturnLucene()
    {
        // arrange

        // act
        var result = _sut.Name;

        // assert
        result.Should().Be("Lucene");
    }

    [Theory]
    [InlineData("tag-only", "tag:abc")]
    [InlineData("tag-only", "  tag:abc  ")]
    [InlineData("tag-only", "tag:\"abc\"")]
    [InlineData("tag-only", " tag:\"abc\"")]
    [InlineData("tag-only", " tag:\"abc  \"")]
    [InlineData("tag-only", " tag:\"   abc  \"")]
    [InlineData("tag-only", " (tag:\"   abc  \")")]
    [InlineData("tag-only", " ((tag:\"   abc  \"))")]
    [InlineData("tag-only", " ((+tag:\"   abc  \"))")]
    [InlineData("tag-only", " +((tag:\"   abc  \"))")]
    [InlineData("tag-only", " +(+(+tag:\"   abc  \"))")]
    [InlineData("tag-min", " -tag:\"   abc  \"")]
    [InlineData("tag-min", " -tag:abc")]
    [InlineData("tag-min", " (-tag:abc)")]
    [InlineData("tag-min", " -(tag:abc)")]
    [InlineData("tag-min", " NOT(tag:abc)")]
    [InlineData("tag-min", " NOT (tag:abc)")]
    [InlineData("tag-min", " NOT tag:abc")]
    [InlineData("tag-min", " !(tag:abc)")]
    [InlineData("tag-min", " (!tag:abc)")]
    [InlineData("tag-min", " !tag:abc")]

    [InlineData("single-word", "aBc@")]
    [InlineData("single-word", " aBc@ ")]
    [InlineData("single-word", "\"aBc@\"")]

    [InlineData("two-word", "word1 word2")]
    [InlineData("two-word", "word1   word2")]
    [InlineData("two-word", "word1 AND  word2")]
    [InlineData("and-or-words", "(word1   word2) OR word33")]
    [InlineData("and-or-words", "(word1  AND word2) OR word33")]
    [InlineData("and-or-words", "word1  AND word2 OR word33")]
    [InlineData("and-or-words-negative", "word1  AND word2 OR -word33")]
    [InlineData("mix1", "word1  AND tag:tagsss OR word33")]
    [InlineData("mix2", "abc.def  word33")]

    [InlineData("range-only", "age:[16 TO 75]")]
    [InlineData("range-only", "  age:[16 TO 75]")]
    [InlineData("range-only", "age:[16 TO 75]  ")]
    [InlineData("range-only", "age:[16   TO   75]")]
    [InlineData("range-only", "age:[  16 TO 75 ]")]
    [InlineData("range-only", "age: [16 TO 75]")]
    [InlineData("range-only", "age : [16 TO 75]")]
    [InlineData("range-only", "age :[16 TO 75]")]
    [InlineData("range-only", "  age:[16 TO 75] ")]
    [InlineData("range-only", "(age:[16 TO 75])")]
    [InlineData("range-only", "+(age:[16 TO 75])")]
    [InlineData("range-only", "(+age:[16 TO 75])")]
    [InlineData("range-only", "+age:[16 TO 75]")]
    // [InlineData("range-only", "(age:[16 TO *])")]
    [InlineData("range-only-excl-left", "age:{16 TO 75]")]
    [InlineData("range-only-excl-right", "age:[16 TO 75}")]
    [InlineData("range-only-excl", "age:{16 TO 75}")]

    [InlineData("range-no-upper", "age:[16 TO *]")]
    [InlineData("range-no-upper2", "age:[16 TO *}")]
    [InlineData("range-no-lower", "age:[* TO 103]")]
    [InlineData("range-no-lower2", "age:[* TO 103}")]

    [InlineData("multi-001", "(+tag:github.com OR +tag:github)")] 
    [InlineData("multi-001", "(tag:github.com OR tag:github)")]
    [InlineData("multi-001", "+(tag:github.com OR tag:github)")]
    [InlineData("multi-001", "+tag:github.com OR +tag:github")]

    [InlineData("multi-002", "(-tag:github.com OR +tag:github)")]
    [InlineData("multi-002", "(-tag:github.com OR tag:github)")]
    [InlineData("multi-002", "+(-tag:github.com OR tag:github)")]

    [InlineData("multi-003", "(+tag:github.com OR -tag:github)")]
    [InlineData("multi-003", "(tag:github.com OR -tag:github)")]
    [InlineData("multi-003", "+(tag:github.com OR -tag:github)")]

    [InlineData("multi-and-001", "(+tag:github.com AND +tag:github)")]
    [InlineData("multi-and-001", "(tag:github.com AND tag:github)")]
    [InlineData("multi-and-001", "+(tag:github.com AND tag:github)")]
    [InlineData("multi-and-001", "+tag:github.com AND +tag:github")]

    [InlineData("multi-and-001", "(+tag:github.com   +tag:github)")]
    [InlineData("multi-and-001", "(tag:github.com  tag:github)")]
    [InlineData("multi-and-001", "+(tag:github.com tag:github)")]
    [InlineData("multi-and-001", "+tag:github.com  +tag:github")]

    [InlineData("multi-and-002", "(-tag:github.com AND +tag:github)")]
    [InlineData("multi-and-002", "(-tag:github.com AND tag:github)")]
    [InlineData("multi-and-002", "+(-tag:github.com AND tag:github)")]
    [InlineData("multi-and-002", "+(-tag:github.com tag:github)")]

    [InlineData("multi-and-003", "(+tag:github.com AND -tag:github)")]
    [InlineData("multi-and-003", "(tag:github.com AND -tag:github)")]
    [InlineData("multi-and-003", "+(tag:github.com AND -tag:github)")]
    [InlineData("multi-and-003", "+(tag:github.com -tag:github)")]

    // [InlineData("tag-wildcard", "tag:github.com*")]
    // [InlineData("tag-wildcard", "tag:github.com *")]
    [InlineData("tag-wildcard-literal", "tag:\"github.com*\"")]
    
    [InlineData("tag-wildcard-start", "tag:*github.com")]
    [InlineData("tag-wildcard-middle-end", "tag:git*.com*")]
    [InlineData("tag-wildcard-end", "tag:github.com*")]
    [InlineData("tag-wildcard-start-end", "tag:*github.com*")]
    [InlineData("tag-question-end", "tag:github.com?")]
    [InlineData("tag-question-start", "tag:?github.com")]

    [InlineData("text-only", "This is Some   Text@ ")]
    [InlineData("text-only", "This is Some Text@")]
    [InlineData("text-only", "  This is Some Text@  ")]
    [InlineData("text-only", "  This is      Some Text@  ")]
    [InlineData("text-only", "  This is      Some^4 Text@  ")] // boosting ignored
    [InlineData("text-only", "  +This +is      Some Text@  ")] // plus doesnt matter

    // [InlineData("text-only", "  This is      Some^ Text@  ")]  // error
    [InlineData("fuzzy", "roam~")]
    [InlineData("fuzzy", "roam~0.8")] 
    [InlineData("fuzzy", "roam~1.0")] 
    [InlineData("Proximity", "\"jakarta apache\"~10")] // Proximity Searches, PhraseQuery
    [InlineData("boosting", "jakarta^4 apache")] // Proximity Searches, PhraseQuery
    public async Task Parse(string outputName, string input)
    {
        // arrange
        
        // act
        IQuery result = _sut.Parse(input);

        // assert
        await Verifier.Verify(new
                {
                    Output = result.ToString(),
                    Model = result,
                },
            _settings).UseTextForParameters(outputName);
    }

    [Theory]
    [InlineData("a:")]
    [InlineData(":")]
    public void Parse_ShouldThrownParseException_WhenInputCannotBeParsed(string input)
    {
        // arrange

        // act
        var act = () => _ = _sut.Parse(input);

        // assert
        act.Should().ThrowExactly<ParseException>();
    }
}