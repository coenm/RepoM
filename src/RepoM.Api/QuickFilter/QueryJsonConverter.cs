namespace RepoM.Api.QuickFilter;

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal sealed class QueryJsonConverter : JsonConverter<IQuery>
{
    private const string TypePropertyName = "type";
    private const string ItemsPropertyName = "items";
    private const string ItemPropertyName = "item";
    private const string TermPropertyName = "term";
    private const string ValuePropertyName = "value";

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
        var type = obj.Value<string>(TypePropertyName) ?? throw new JsonSerializationException("Missing 'type' property.");

        return type switch
        {
            "true" => TrueQuery.Instance,
            "false" => FalseQuery.Instance,
            "and" => ReadAndQuery(obj),
            "or" => ReadOrQuery(obj),
            "not" => ReadNotQuery(obj),
            "freetext" => new FreeText(ReadStringProperty(obj, ValuePropertyName)),
            "simpleterm" => new SimpleTerm(
                ReadStringProperty(obj, TermPropertyName),
                ReadStringProperty(obj, ValuePropertyName)),
            "startswithterm" => new StartsWithTerm(
                ReadStringProperty(obj, TermPropertyName),
                ReadStringProperty(obj, ValuePropertyName)),
            _ => throw new JsonSerializationException($"Unknown query type '{type}'."),
        };
    }

    private static AndQuery ReadAndQuery(JObject obj)
    {
        var items = obj[ItemsPropertyName] as JArray ?? [];
        return new AndQuery(items.Select(ReadQuery).ToArray());
    }

    private static OrQuery ReadOrQuery(JObject obj)
    {
        var items = obj[ItemsPropertyName] as JArray ?? [];
        return new OrQuery(items.Select(ReadQuery).ToArray());
    }

    private static NotQuery ReadNotQuery(JObject obj)
    {
        var item = obj[ItemPropertyName] ?? throw new JsonSerializationException("Missing 'item' in NotQuery.");
        return new NotQuery(ReadQuery(item));
    }

    private static string ReadStringProperty(JObject obj, string propertyName)
    {
        return obj.Value<string>(propertyName) ?? string.Empty;
    }

    private static void WriteQuery(JsonWriter writer, IQuery query)
    {
        switch (query)
        {
            case TrueQuery:
                writer.WriteStartObject();
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("true");
                writer.WriteEndObject();
                break;

            case FalseQuery:
                writer.WriteStartObject();
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("false");
                writer.WriteEndObject();
                break;

            case AndQuery and:
                writer.WriteStartObject();
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("and");
                writer.WritePropertyName(ItemsPropertyName);
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
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("or");
                writer.WritePropertyName(ItemsPropertyName);
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
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("not");
                writer.WritePropertyName(ItemPropertyName);
                WriteQuery(writer, not.Item);
                writer.WriteEndObject();
                break;

            case FreeText freeText:
                writer.WriteStartObject();
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("freetext");
                writer.WritePropertyName(ValuePropertyName);
                writer.WriteValue(freeText.Value);
                writer.WriteEndObject();
                break;

            case SimpleTerm simpleTerm:
                writer.WriteStartObject();
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("simpleterm");
                writer.WritePropertyName(TermPropertyName);
                writer.WriteValue(simpleTerm.Term);
                writer.WritePropertyName(ValuePropertyName);
                writer.WriteValue(simpleTerm.Value);
                writer.WriteEndObject();
                break;

            case StartsWithTerm startsWithTerm:
                writer.WriteStartObject();
                writer.WritePropertyName(TypePropertyName);
                writer.WriteValue("startswithterm");
                writer.WritePropertyName(TermPropertyName);
                writer.WriteValue(startsWithTerm.Term);
                writer.WritePropertyName(ValuePropertyName);
                writer.WriteValue(startsWithTerm.Value);
                writer.WriteEndObject();
                break;

            default:
                throw new JsonSerializationException($"Unknown IQuery type '{query.GetType().Name}'.");
        }
    }
}