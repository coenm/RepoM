namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class Predicate : EvaluateObject
{
    public bool DefaultValue { get; set; }

    public static implicit operator Predicate(string content)
    {
        return new Predicate { Value = content, };
    }

    public override string ToString()
    {
        return $"Predicate {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<bool> EvaluateAsync(ITemplateEvaluator instance)
    {
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