namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class ConfigurationFileNotFoundException : Exception
{
    public ConfigurationFileNotFoundException(string filename)
    {
        Filename = filename;
    }

    protected ConfigurationFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Filename = info.GetString(nameof(Filename))!;
    }

    public ConfigurationFileNotFoundException(string filename, string message) : base(message)
    {
        Filename = filename;
    }

    public ConfigurationFileNotFoundException(string filename, string message, Exception innerException) : base(message, innerException)
    {
        Filename = filename;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Filename), Filename);
    }

    public string Filename { get; private set; }
}