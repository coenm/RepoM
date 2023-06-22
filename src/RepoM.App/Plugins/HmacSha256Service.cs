namespace RepoM.App.Plugins;

using System.IO;
using System.Linq;
using System.Security.Cryptography;

internal class HmacSha256Service : IHmacService
{
    private const int KEY_LENGTH = 64;

    public byte[] GetHmac(Stream input)
    {
        byte[] key = RandomNumberGenerator.GetBytes(KEY_LENGTH);
        using var calculator = new HMACSHA256(key);
        return key.Concat(calculator.ComputeHash(input)).ToArray(); // oops :-)
    }

    public bool ValidateHmac(Stream input, in byte[] hmac)
    {
        if (hmac.Length != KEY_LENGTH + HMACSHA256.HashSizeInBytes)
        {
            return false;
        }
        
        using var calculator = new HMACSHA256(hmac[..KEY_LENGTH].ToArray());
        var computedHash = calculator.ComputeHash(input);
        return computedHash.SequenceEqual(hmac[KEY_LENGTH..]);
    }
}