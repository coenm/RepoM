namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Deserialization;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class BoolToStringJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(string) == objectType;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var str = token.Value<string>();

        if ("true".Equals(str, StringComparison.OrdinalIgnoreCase))
        {
            return "true";
        }

        if ("false".Equals(str, StringComparison.OrdinalIgnoreCase))
        {
            return "false";
        }

        return str;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // intentionally
        throw new NotImplementedException();
    }
}