namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string filename)
    {
        Filename = filename;
    }

    protected InvalidConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Filename = info.GetString(nameof(Filename))!;
    }

    public InvalidConfigurationException(string filename, string message) : base(message)
    {
        Filename = filename;
    }

    public InvalidConfigurationException(string filename, string message, Exception innerException) : base(message, innerException)
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