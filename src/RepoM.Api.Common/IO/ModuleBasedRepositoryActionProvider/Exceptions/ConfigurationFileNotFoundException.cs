namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Exceptions;

using System;
using System.Runtime.Serialization;

public class ConfigurationFileNotFoundException : Exception
{
    public ConfigurationFileNotFoundException(string filename)
    {
        Filename = filename;
    }

    protected ConfigurationFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Filename = string.Empty; //todo
    }

    public ConfigurationFileNotFoundException(string filename, string message) : base(message)
    {
        Filename = filename;
    }

    public ConfigurationFileNotFoundException(string filename, string message, Exception innerException) : base(message, innerException)
    {
        Filename = filename;
    }

    public string Filename { get; private set; }
}