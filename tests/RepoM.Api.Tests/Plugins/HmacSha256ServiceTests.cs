namespace RepoM.Api.Tests.Plugins;

using System.IO;
using System.Linq;
using FluentAssertions;
using RepoM.Api.Plugins;
using Xunit;

public class HmacSha256ServiceTests
{
    private readonly HmacSha256Service _sut;
    private readonly Stream _stream1;
    private readonly Stream _stream2;
    
    public HmacSha256ServiceTests()
    {
        _sut = new HmacSha256Service();
        _stream1 = new MemoryStream(new byte[] { 0x00, });
        _stream2 = new MemoryStream(new byte[] { 0x01, });
    }
    
    [Fact]
    public void GetHmac_ShouldReturnBytesWithExactLength()
    {
        // arrange

        // act
        var result = _sut.GetHmac(_stream1);

        // assert
        result.Should().HaveCount(96);
    }

    [Fact]
    public void GetHmac_ShouldReturnDifferentResultOnSameStream()
    {
        // arrange
        var firstHmac = _sut.GetHmac(_stream1);
        _stream1.Position = 0;

        // act
        var result = _sut.GetHmac(_stream1);

        // assert
        firstHmac.Should().NotBeEquivalentTo(result);
    }

    [Fact]
    public void ValidateHmac_ShouldBeTrue_WhenValidatingSameStream()
    {
        // arrange
        var hmacValue = _sut.GetHmac(_stream1);
        _stream1.Position = 0;

        // act
        var result = _sut.ValidateHmac(_stream1, hmacValue);

        // assert
        result.Should().BeTrue();
    }


    [Fact] public void ValidateHmac_ShouldBeFalse_WhenInvalidHmacValueProvided()
    {
        // arrange
        var hmacValue = _sut.GetHmac(_stream1);
        _stream1.Position = 0;

        // act
        hmacValue = hmacValue.Reverse().ToArray(); // yes, there is a possibility that this exactly the same. Not very likely though.
        var result = _sut.ValidateHmac(_stream1, hmacValue);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateHmac_ShouldBeFalse_WhenHmacLengthIsNotCorrect1()
    {
        // arrange
        var hmacValue = _sut.GetHmac(_stream1);
        _stream1.Position = 0;

        // act
        var result = _sut.ValidateHmac(_stream1, hmacValue.Skip(1).ToArray());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateHmac_ShouldBeFalse_WhenHmacLengthIsNotCorrect2()
    {
        // arrange
        var hmacValue = _sut.GetHmac(_stream1);
        _stream1.Position = 0;

        // act
        var result = _sut.ValidateHmac(_stream1, hmacValue.Concat(new byte[1]).ToArray());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateHmac_ShouldBeFalse_WhenValidatingAgainstOtherStream()
    {
        // arrange
        var hmacValue = _sut.GetHmac(_stream1);

        // act
        var result = _sut.ValidateHmac(_stream2, hmacValue);

        // assert
        result.Should().BeFalse();
    }
}