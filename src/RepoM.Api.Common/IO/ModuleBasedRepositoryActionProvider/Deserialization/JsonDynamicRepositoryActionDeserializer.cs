namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Deserialization;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public class JsonDynamicRepositoryActionDeserializer
{
    private readonly ActionDeserializerComposition _deserializers;
    private static readonly JsonSerializer _jsonSerializer = new()
        {
            Converters =
                {
                    new BoolToStringJsonConverter(),
                },
        };
    private static readonly JsonLoadSettings _jsonLoadSettings = new()
        {
            CommentHandling = CommentHandling.Ignore,
        };

    public JsonDynamicRepositoryActionDeserializer(ActionDeserializerComposition deserializers)
    {
        _deserializers = deserializers ?? throw new ArgumentNullException(nameof(deserializers));
    }

    public RepositoryActionConfiguration Deserialize(string rawContent)
    {
        var jsonObject = JObject.Parse(rawContent, _jsonLoadSettings);

        var configuration = new RepositoryActionConfiguration();

        JToken? token = jsonObject["version"];
        var version = DeserializeVersion(token);

        if (version == 1)
        {
            token = jsonObject["redirect"];
            configuration.Redirect = DeserializeRedirect(token);

            token = jsonObject["repository-specific-env-files"];
            configuration.RepositorySpecificEnvironmentFiles.AddRange(TryDeserializeEnumerable<FileReference>(token));

            token = jsonObject["repository-specific-config-files"];
            configuration.RepositorySpecificConfigFiles.AddRange(TryDeserializeEnumerable<FileReference>(token));

            token = jsonObject["variables"];
            configuration.Variables.AddRange(TryDeserializeEnumerable<Variable>(token));

            token = jsonObject["repository-tags"];
            DeserializeRepositoryTags(token, ref configuration);

            token = jsonObject["repository-actions"];
            DeserializeRepositoryActions(token, configuration);
        }

        return configuration;
    }

    private void DeserializeRepositoryActions(JToken? repositoryActionsToken, RepositoryActionConfiguration configuration)
    {
        if (repositoryActionsToken == null)
        {
            return;
        }

        JToken? actions = repositoryActionsToken.SelectToken("actions");
        if (actions != null)
        {
            JToken? jTokenVariables = repositoryActionsToken.SelectToken("variables");
            configuration.ActionsCollection.Variables.AddRange(TryDeserializeEnumerable<Variable>(jTokenVariables));
            repositoryActionsToken = actions;
        }

        IList<JToken> repositoryActions = repositoryActionsToken.Children().ToList();
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

            RepositoryAction? customAction = _deserializers.DeserializeSingleAction(typeValue!, variable, _jsonSerializer);
            if (customAction == null)
            {
                continue;
            }

            configuration.ActionsCollection.Actions.Add(customAction);
        }
    }

    private static void DeserializeRepositoryTags(JToken? repositoryTagsToken, ref RepositoryActionConfiguration configuration)
    {
        if (repositoryTagsToken == null)
        {
            return;
        }

        JToken? tagsToken = repositoryTagsToken.SelectToken("tags");
        if (tagsToken != null)
        {
            JToken? token = repositoryTagsToken.SelectToken("variables");
            configuration.TagsCollection.Variables.AddRange(TryDeserializeEnumerable<Variable>(token));
            configuration.TagsCollection.Tags.AddRange(TryDeserializeEnumerable<RepositoryActionTag>(tagsToken));
        }
        else
        {
            configuration.TagsCollection.Tags.AddRange(TryDeserializeEnumerable<RepositoryActionTag>(repositoryTagsToken));
        }
    }

    private static IEnumerable<T> TryDeserializeEnumerable<T>(JToken? token)
    {
        if (token == null)
        {
            yield break;
        }

        IList<JToken> files = token.Children().ToList();
        foreach (JToken file in files)
        {
            T? obj = file.ToObject<T>(_jsonSerializer);
            if (obj != null)
            {
                yield return obj;
            }
        }
    }

    private static Redirect? DeserializeRedirect(JToken? redirectToken)
    {
        if (redirectToken == null)
        {
            return null;
        }

        if (redirectToken.Type != JTokenType.String)
        {
            return redirectToken.ToObject<Redirect>(_jsonSerializer);
        }

        var redirectValue = redirectToken.Value<string>();

        return new Redirect()
            {
                Filename = redirectValue ?? string.Empty,
            };
    }

    private static int DeserializeVersion(JToken? versionToken)
    {
        const int DEFAULT_VERSION = 1;
        if (versionToken == null)
        {
            return DEFAULT_VERSION;
        }

        var version = DEFAULT_VERSION;

        if (versionToken.Type == JTokenType.Integer)
        {
            version = versionToken.Value<int>();
            return version < 1 ? DEFAULT_VERSION : version;
        }

        if (versionToken.Type == JTokenType.String)
        {
            var versionString = versionToken.Value<string>();
            if (string.IsNullOrWhiteSpace(versionString))
            {
                return DEFAULT_VERSION;
            }

            if (int.TryParse(versionString, out version))
            {
                return version < 1 ? DEFAULT_VERSION : version;
            }
        }

        return DEFAULT_VERSION;
    }
}