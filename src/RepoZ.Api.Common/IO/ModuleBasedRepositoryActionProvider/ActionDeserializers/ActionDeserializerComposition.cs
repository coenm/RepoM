namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public class ActionDeserializerComposition
{
    private readonly IActionDeserializer[] _deserializers;

    public ActionDeserializerComposition(IEnumerable<IActionDeserializer> deserializers)
    {
        _deserializers = deserializers?.Where(x => x != null).ToArray() ?? throw new ArgumentNullException(nameof(deserializers));
    }

    public RepositoryAction? DeserializeSingleAction(string type, JToken jToken)
    {
        IActionDeserializer? deserializer = _deserializers.FirstOrDefault(x => x.CanDeserialize(type));

        RepositoryAction? result = deserializer?.Deserialize(jToken, this);

        if (result == null)
        {
            return null;
        }

        JToken? multiSelectEnabledToken = jToken["multi-select-enabled"];

        if (multiSelectEnabledToken != null)
        {
            var multiSelectEnabledValue = multiSelectEnabledToken.Value<string>();
            if (!string.IsNullOrWhiteSpace(multiSelectEnabledValue))
            {
                result.MultiSelectEnabled = multiSelectEnabledValue!;
            }
        }

        return result;
    }
}