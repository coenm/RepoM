namespace RepoM.Plugin.LuceneQueryParser.Tests;

using System.Threading.Tasks;
using Argon;
using RepoM.Plugin.LuceneQueryParser;
using RepoM.Plugin.LuceneQueryParser.Plugin.Clause;
using VerifyTests;
using VerifyXunit;
using Xunit;

// https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
[UsesVerify]
public class RepositoryIndexTests
{
    private readonly LuceneQueryParser1 _sut;
    private readonly VerifySettings _settings;

    public RepositoryIndexTests()
    {
        _sut = new LuceneQueryParser1();

        _settings = new VerifySettings();
        _settings.AddExtraSettings(settings =>
            {
                settings.DefaultValueHandling = DefaultValueHandling.Include;
                // settings.NullValueHandling = NullValueHandling.Include;
                settings.TypeNameHandling = TypeNameHandling.Auto;

            });
        _settings.DisableRequireUniquePrefix();
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

    // [InlineData("single-word", "aBc@")]
    // [InlineData("single-word", " aBc@ ")]
    // [InlineData("single-word", " \"aBc@\" ")]
    // [InlineData("single-word", " ((\"aBc@\")) ")]
    // // [InlineData("single-word", " +(\"aBc@\") ")]
    // // [InlineData("single-word", " (+\"aBc@\") ")]

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

    // // [InlineData("text-only", "This is Some   Text@ ")]
    // // [InlineData("text-only", "This is Some Text@")]
    // // [InlineData("text-only", "  This is Some Text@  ")]
    // // [InlineData("text-only", "  This is      Some Text@  ")]
    // // [InlineData("text-only", "  This is      Some^4 Text@  ")] // boosting ignored
    // // // [InlineData("text-only", "  This is      Some^ Text@  ")]  // error
    // // [InlineData("text-only", "  +This +is      Some Text@  ")] // plus doesnt matter
    // //
    

    //
    // // // wildcard not yet implemented
    // [InlineData("wildcard-q-star", "te?t*")]
    // [InlineData("wildcard-q", "te?t")]
    // // [InlineData("wildcard-q2", "te?t abc")]
    // [InlineData("no-wildcard-q", "\"te?t\"")]
    // [InlineData("wildcard-start", "*ext")]
    // [InlineData("wildcard", "te*t")]
    // [InlineData("wildcard", " te*t  ")]
    // [InlineData("no-wildcard", " \"te*t\"  ")] // * is not wildcard because inside quotes
    // //
    // // // fuzzy search FuzzyQuery
    // [InlineData("fuzzy", "roam~")]
    // [InlineData("fuzzy08", "roam~0.8")] // same as above due to removing 'MaxEdits' property in verify
    // [InlineData("fuzzy10", "roam~1.0")] // same as above due to removing 'MaxEdits' property in verify
    //
    // // //[InlineData("Proximity", "\"jakarta apache\"~10")] // Proximity Searches, PhraseQuery
    // // // [InlineData("boosting", "jakarta^4 apache")] // Proximity Searches, PhraseQuery
    // //
    // [InlineData("multi-001", "(+tag:github.com OR +tag:github)", false)] // or, two tags
    // [InlineData("multi-001", "+tag:github.com OR +tag:github", true)] // or, two tags
    // [InlineData("multi-001", "+tag:github.com +tag:github", false)] // or, two tags
    // [InlineData("multi-001", "+tag:github.com +tag:github", true)] // or, two tags
    // // [InlineData("multi-001", "(tag:github.com OR tag:github)")] // or, two tags
    // // [InlineData("multi-001", "(+tag:github.com OR +tag:github)")] // or, two tags
    // // [InlineData("multi-001.1", "+(tag:github.com OR tag:github)")] // or, two tags
    // // [InlineData("multi-001.2", "-(tag:github.com OR tag:github)")] // negative or, two tags
    // // [InlineData("multi-001.3", "(-tag:github.com AND -tag:github)")] // negative or, two tags
    // //
    // // [InlineData("multi-002", "is:pinned repom")]
    // // [InlineData("multi-002", "is:pinned AND repom")]
    // // [InlineData("multi-002", "is:pinned OR repom")] 
}