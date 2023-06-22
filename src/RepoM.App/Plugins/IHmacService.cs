namespace RepoM.App.Plugins;

using System.IO;

internal interface IHmacService
{
    byte[] GetHmac(Stream input);

    bool ValidateHmac(Stream input, in byte[] hmac);
}