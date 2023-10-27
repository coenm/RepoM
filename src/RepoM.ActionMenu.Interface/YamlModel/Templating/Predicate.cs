namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class Predicate : EvaluateObjectBase
{
    protected bool? StaticValue; // todo slaat nergens op?!

    public bool DefaultValue { get; set; }

    public static implicit operator Predicate(bool content)
    {
        return content
            ? new Predicate { Value = "true", StaticValue = content, }
            : new Predicate { Value = "false", StaticValue = content, };
    }

    public static implicit operator Predicate(string content)
    {
        bool? staticValue = null;

        if ("true".Equals(content, StringComparison.CurrentCultureIgnoreCase))
        {
            staticValue = true;
        }
        else if ("false".Equals(content, StringComparison.CurrentCultureIgnoreCase))
        {
            staticValue = false;
        }

        return new Predicate { Value = content, StaticValue =  staticValue, };
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