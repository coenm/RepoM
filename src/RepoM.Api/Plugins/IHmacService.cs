namespace RepoM.Api.Plugins;

using System.IO;

public interface IHmacService
{
    byte[] GetHmac(Stream input);

    bool ValidateHmac(Stream input, in byte[] hmac);
}