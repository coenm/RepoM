namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

[Obsolete("Old action menu")]
public class ActionForEachV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return RepositoryActionForEachV1.TYPE.Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, actionDeserializer, jsonSerializer);
    }

    private static RepositoryActionForEachV1? Deserialize(JToken token, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        RepositoryActionForEachV1? result = token.ToObject<RepositoryActionForEachV1>(jsonSerializer);

        if (result == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(result.Variable))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(result.Enumerable))
        {
            return null;
        }
        
        JToken? actions = token.SelectToken("actions");
        if (actions == null)
        {
            return null;
        }

        result.Actions.Clear();
        
        IList<JToken> repositoryActions = actions.Children().ToList();
        foreach (JToken variable in repositoryActions)
        {
            JToken? typeToken = variable["type"];
            if (typeToken == null)
            {
                continue;
            }
        
            var typeValue = typeToken.Value<string>();
            if (string.IsNullOrWhiteSpace(typeValue))
            {
                continue;
            }
        
            RepositoryAction? customAction = actionDeserializer.DeserializeSingleAction(typeValue!, variable, jsonSerializer);
            if (customAction == null)
            {
                continue;
            }
            
            result.Actions.Add(customAction);
        }

        return result;
    }
}