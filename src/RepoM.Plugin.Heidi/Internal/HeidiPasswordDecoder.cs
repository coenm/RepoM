namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class HeidiPasswordDecoder : IHeidiPasswordDecoder
{
    private HeidiPasswordDecoder()
    {
    }

    public static HeidiPasswordDecoder Instance { get; } = new();

    public string DecodePassword(ReadOnlySpan<char> input)
    {
        if (input.Length is 0 or 1)
        {
            return string.Empty;
        }

        if (input.Length < 3)
        {
            throw new InvalidPasswordException();
        }

        if (input.Length % 2 != 1)
        {
            throw new InvalidPasswordException();
        }

        if (!int.TryParse(input[^1..], out var shift))
        {
            throw new InvalidPasswordException();
        }

        try
        {
            IEnumerable<char> chars = StringToByteArray(input[..^1], shift);
            var result = new StringBuilder(input[..^1].Length / 2);

            foreach (var c in chars)
            {
                result.Append(c);
            }

            return result.ToString();
        }
        catch (Exception e)
        {
            throw new InvalidPasswordException("Invalid password", e);
        }
    }

    private static IEnumerable<char> StringToByteArray(ReadOnlySpan<char> hex, int shift)
    {
        var hexString = hex.ToString();
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x =>
                             Convert.ToChar(
                                Convert.ToInt32(
                                  Convert.ToByte(hexString.Substring(x, 2), 16)) - shift));
    }
}