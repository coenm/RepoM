namespace RepoM.Api.QuickFilter;

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal sealed class QueryJsonConverter : JsonConverter<IQuery>
{
    public override IQuery? ReadJson(JsonReader reader, Type objectType, IQuery? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        return ReadQuery(token);
    }

    public override void WriteJson(JsonWriter writer, IQuery? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        WriteQuery(writer, value);
    }

    private static IQuery ReadQuery(JToken token)
    {
        if (token.Type != JTokenType.Object)
        {
            throw new JsonSerializationException("Expected JSON object for IQuery.");
        }

        var obj = (JObject)token;
        var type = obj.Value<string>("type") ?? throw new JsonSerializationException("Missing 'type' property.");

        return type switch
        {
            "true" => TrueQuery.Instance,
            "false" => FalseQuery.Instance,
            "and" => ReadAndQuery(obj),
            "or" => ReadOrQuery(obj),
            "not" => ReadNotQuery(obj),
            "freetext" => new FreeText(obj.Value<string>("value") ?? string.Empty),
            "simpleterm" => new SimpleTerm(
                obj.Value<string>("term") ?? string.Empty,
                obj.Value<string>("value") ?? string.Empty),
            "startswithterm" => new StartsWithTerm(
                obj.Value<string>("term") ?? string.Empty,
                obj.Value<string>("value") ?? string.Empty),
            _ => throw new JsonSerializationException($"Unknown query type '{type}'."),
        };
    }

    private static AndQuery ReadAndQuery(JObject obj)
    {
        var items = obj["items"] as JArray ?? [];
        return new AndQuery(items.Select(ReadQuery).ToArray());
    }

    private static OrQuery ReadOrQuery(JObject obj)
    {
        var items = obj["items"] as JArray ?? [];
        return new OrQuery(items.Select(ReadQuery).ToArray());
    }

    private static NotQuery ReadNotQuery(JObject obj)
    {
        var item = obj["item"] ?? throw new JsonSerializationException("Missing 'item' in NotQuery.");
        return new NotQuery(ReadQuery(item));
    }

    private static void WriteQuery(JsonWriter writer, IQuery query)
    {
        switch (query)
        {
            case TrueQuery:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("true");
                writer.WriteEndObject();
                break;

            case FalseQuery:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("false");
                writer.WriteEndObject();
                break;

            case AndQuery and:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("and");
                writer.WritePropertyName("items");
                writer.WriteStartArray();
                foreach (IQuery item in and.Items)
                {
                    WriteQuery(writer, item);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
                break;

            case OrQuery or:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("or");
                writer.WritePropertyName("items");
                writer.WriteStartArray();
                foreach (IQuery item in or.Items)
                {
                    WriteQuery(writer, item);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
                break;

            case NotQuery not:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("not");
                writer.WritePropertyName("item");
                WriteQuery(writer, not.Item);
                writer.WriteEndObject();
                break;

            case FreeText freeText:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("freetext");
                writer.WritePropertyName("value");
                writer.WriteValue(freeText.Value);
                writer.WriteEndObject();
                break;

            case SimpleTerm simpleTerm:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("simpleterm");
                writer.WritePropertyName("term");
                writer.WriteValue(simpleTerm.Term);
                writer.WritePropertyName("value");
                writer.WriteValue(simpleTerm.Value);
                writer.WriteEndObject();
                break;

            case StartsWithTerm startsWithTerm:
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("startswithterm");
                writer.WritePropertyName("term");
                writer.WriteValue(startsWithTerm.Term);
                writer.WritePropertyName("value");
                writer.WriteValue(startsWithTerm.Value);
                writer.WriteEndObject();
                break;

            default:
                throw new JsonSerializationException($"Unknown IQuery type '{query.GetType().Name}'.");
        }
    }
}