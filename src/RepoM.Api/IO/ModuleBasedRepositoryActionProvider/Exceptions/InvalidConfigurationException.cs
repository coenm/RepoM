namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Exceptions;

using System;
using System.Runtime.Serialization;

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string filename)
    {
        Filename = filename;
    }

    protected InvalidConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Filename = string.Empty; //todo
    }

    public InvalidConfigurationException(string filename, string message) : base(message)
    {
        Filename = filename;
    }

    public InvalidConfigurationException(string filename, string message, Exception innerException) : base(message, innerException)
    {
        Filename = filename;
    }

    public string Filename { get; private set; }
}