namespace RepoM.Plugin.Heidi.Internal;

using System;

internal interface IHeidiPasswordDecoder
{
    string DecodePassword(ReadOnlySpan<char> input);
}