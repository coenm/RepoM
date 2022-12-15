namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using RepoM.Api.IO.Variables;
using ExpressionStringEvaluator.VariableProviders;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

interface IItem
{
}

interface IItemHandler<in T> where T : IItem
{
    object? Handle(T item, object? value);
}

struct ArraySelector : IItem
{
    public ArraySelector(int index)
    {
        Index = index;
    }

    public int Index { get; set; }
}

struct PropertySelector : IItem
{
    public PropertySelector(string property)
    {
        Property = property;
    }

    public string Property { get; set; }
}

class ArrayHandler : IItemHandler<ArraySelector>
{
    public object? Handle(ArraySelector item, object? value)
    {
        if (value is IList list)
        {
            if (list.Count > item.Index)
            {
                return list[item.Index];
            }
        }

        return null;
    }
}

class PropertyHandler : IItemHandler<PropertySelector>
{
    public object? Handle(PropertySelector item, object? value)
    {
        if (value is ExpandoObject eo)
        {
            return eo.SingleOrDefault(pair => pair.Key == item.Property).Value;
        }

        return null;
    }
}

public class RepoMVariableProvider : IVariableProvider
{
    private const string PREFIX = "var.";

    /// <inheritdoc cref="IVariableProvider.CanProvide"/>
    public bool CanProvide(string key)
    {
        if (!key.StartsWith(PREFIX, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        var prefixLength = PREFIX.Length;
        if (key.Length <= prefixLength)
        {
            return false;
        }

        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        return !string.IsNullOrWhiteSpace(envKey);
    }
    
    /// <inheritdoc cref="IVariableProvider.Provide"/>
    public object? Provide(string key, string? arg)
    {
        var prefixLength = PREFIX.Length;
        var envKey = key.Substring(prefixLength, key.Length - prefixLength);
        var envSearchKey = envKey;
        var index = envKey.IndexOfAny(new [] { '.', '[', });
        if (index > 0)
        {
            envSearchKey = envKey.Substring(0, index);
        }

        Scope? scope = RepoMVariableProviderStore.VariableScope.Value;

        while (true)
        {
            if (scope == null)
            {
                return null;
            }

            if (TryGetValueFromScope(scope, envSearchKey, out var result))
            {
                if (index < 0)
                {
                    return result;
                }

                var selectors = FindSelectors(envKey.Substring(index)).ToList();
                object? r = result;

                var ph = new PropertyHandler();
                var ah = new ArrayHandler();

                foreach (IItem selector in selectors)
                {
                    if (selector is PropertySelector ps)
                    {
                        r = ph.Handle(ps, r);
                    }

                    else if (selector is ArraySelector @as)
                    {
                        r = ah.Handle(@as, r);
                    }
                }

                return r;
            }

            scope = scope.Parent;
        }
    }

    private IEnumerable<IItem> FindSelectors(string selector)
    {
        var dots = selector.TrimStart('.').Split('.').ToList();
        foreach (var dot in dots)
        {
            var arrays = dot.Split('[').ToList();
            
            if (arrays.Count > 1)
            {
                if (!string.IsNullOrWhiteSpace(arrays[0]))
                {
                    yield return new PropertySelector(arrays[0]);
                }

                // for now, only first
                var index = arrays[1].TrimEnd(']');
                yield return new ArraySelector(int.Parse(index));
            }
            else
            {
                yield return new PropertySelector(arrays[0]);
            }
        }
    }

    private static bool TryGetValueFromScope(in Scope scope, string key, out object? value)
    {
        EvaluatedVariable? var = scope.Variables.FirstOrDefault(x => key.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));

        if (var != null)
        {
            value = var.Value;
            return var.Value != null;
        }

        value = null;
        return false;
    }
}