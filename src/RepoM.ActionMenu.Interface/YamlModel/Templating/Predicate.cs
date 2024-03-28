namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class Predicate : EvaluateObjectBase
{
    protected bool? StaticValue;

    public bool DefaultValue { get; set; }

    public static implicit operator Predicate(bool value)
    {
        return value
            ? new Predicate { Value = "true", StaticValue = value, }
            : new Predicate { Value = "false", StaticValue = value, };
    }

    public static implicit operator Predicate(string value)
    {
        bool? staticValue = null;

        if ("true".Equals(value, StringComparison.CurrentCultureIgnoreCase))
        {
            staticValue = true;
        }
        else if ("false".Equals(value, StringComparison.CurrentCultureIgnoreCase))
        {
            staticValue = false;
        }

        return new Predicate { Value = value, StaticValue =  staticValue, };
    }

    public override string ToString()
    {
        return $"{nameof(Predicate)} {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<bool> EvaluateAsync(ITemplateEvaluator instance)
    {
        if (StaticValue.HasValue)
        {
            return StaticValue.Value;
        }

        var result = await instance.EvaluateAsync(Value).ConfigureAwait(false);
        return ToBool(result);
    }

    protected bool ToBool(object value)
    {
        if (value == null)
        {
            return DefaultValue;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is int intValue)
        {
            return Convert.ToBoolean(intValue);
        }

        if (value is string stringValue)
        {
            if (bool.TryParse(stringValue, out var stringBoolValue))
            {
                return stringBoolValue;
            }

            if (int.TryParse(stringValue, out int stringIntValue))
            {
                return Convert.ToBoolean(stringIntValue);
            }
        }

        return DefaultValue;
    }
}