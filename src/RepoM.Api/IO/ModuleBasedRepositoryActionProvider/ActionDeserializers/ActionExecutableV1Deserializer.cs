namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

[Obsolete("Old action menu")]
public class ActionExecutableV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return RepositoryActionExecutableV1.TYPE.Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionExecutableV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        RepositoryActionExecutableV1? result = jToken.ToObject<RepositoryActionExecutableV1>(jsonSerializer);

        if (result == null)
        {
            return null;
        }

        if (result.Executables.Any())
        {
            return result;
        }

        JToken? executableToken = jToken.SelectToken("executable");

        if (executableToken == null)
        {
            return result;
        }

        if (executableToken.Type != JTokenType.String)
        {
            return result;
        }

        var executable = executableToken.Value<string>();
        if (string.IsNullOrWhiteSpace(executable))
        {
            return result;
        }

        result.Executables = new List<string>(1)
            {
                executable!,
            };

        return result;
    }
}