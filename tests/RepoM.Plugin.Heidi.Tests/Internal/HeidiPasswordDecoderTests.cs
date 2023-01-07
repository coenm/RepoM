namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using FluentAssertions;
using RepoM.Plugin.Heidi.Internal;
using Xunit;

public class HeidiPasswordDecoderTests
{
    private readonly HeidiPasswordDecoder _sut;

    public HeidiPasswordDecoderTests()
    {
        _sut = HeidiPasswordDecoder.Instance;
    }

    [Theory]
    [InlineData("6768696", "abc")]
    [InlineData("6465663", "abc")]
    [InlineData("656667654", "abca")]
    [InlineData("276", "!")]
    public void DecodePassword_ShouldDecode_WhenInputIsValid(string input, string expectedPlainPassword)
    {
        // arrange
        
        // act
        var result = _sut.DecodePassword(input);
        
        // assert
        result.Should().Be(expectedPlainPassword);
    }

    [Theory]
    [InlineData("")] // < 3
    [InlineData(" ")] // < 3
    [InlineData("  ")] // < 3
    [InlineData("1234")] // even
    [InlineData("123456")] // even
    [InlineData("12345678")] // even
    [InlineData("123456A")] // last char is not int
    [InlineData("123456x")] // last char is not int
    [InlineData("123456a")] // last char is not int
    [InlineData("123456$")] // last char is not int
    [InlineData("XX1")] // XX is invalid hex
    public void DecodePassword_ShouldThrow_WhenInputInvalid(string input)
    {
        // arrange

        // act
        Action act = () => _ = _sut.DecodePassword(input);

        // assert
        act.Should().Throw<InvalidPasswordException>();
    }
}