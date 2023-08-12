namespace RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

internal static class PropertyInfoExtensions
{
    public static string SanitizePropertyName(this PropertyInfo propertyInfo)
    {
        // not sure if this is fully correct but fix this on the fly.
        var sb = new StringBuilder();

        if (propertyInfo.Name.Length == 1)
        {
            return propertyInfo.Name.ToLower();
        }

        sb.Append(propertyInfo.Name[0].ToString().ToLower());

        foreach (var c in propertyInfo.Name[1 ..])
        {
            if (c is >= 'A' and <= 'Z')
            {
                sb.Append('-');
                sb.Append((char)(c + 32));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public static bool IsObsolete(this PropertyInfo propertyInfo)
    {
        Attribute[] attributes = propertyInfo.GetCustomAttributes().ToArray();
        return attributes.OfType<ObsoleteAttribute>().SingleOrDefault() != null;
    }

    public static string PropertyAttributesToString(this PropertyInfo propertyInfo)
    {
        Attribute[] attributes = propertyInfo.GetCustomAttributes().ToArray();

        if (attributes.Length == 0)
        {
            return string.Empty;
        }

        var props = new List<string>(3)
            {
                attributes.OfType<RequiredAttribute>().Any() ? "required" : "optional",
            };

        if (attributes.Any(attribute => attribute is EvaluatedPropertyAttribute))
        {
            props.Add("evaluated");
        }

        if (attributes.Any(attribute => attribute is UiConfiguredAttribute))
        {
            props.Add("UI configured");
        }
        else if (attributes.Any(attribute => attribute is ManualConfiguredAttribute))
        {
            props.Add("Manual configured");
        }

        PropertyTypeAttribute? propertyTypeAttribute = attributes.OfType<PropertyTypeAttribute>().SingleOrDefault();
        if (propertyTypeAttribute != null)
        {
            props.Add(propertyTypeAttribute.Type.Name.ToLower());
        }

        PropertyDefaultBoolValueAttribute? propertyDefaultBoolValue = attributes.OfType<PropertyDefaultBoolValueAttribute>().SingleOrDefault();
        if (propertyDefaultBoolValue != null)
        {
            props.Add("default: `" + propertyDefaultBoolValue.DefaultValue.ToString().ToLower() + "`");
        }

        if (props.Count > 0)
        {
            return " (" + string.Join(", ", props) + ")";
        }

        return string.Empty;
    }
}