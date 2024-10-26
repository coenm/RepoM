namespace UiTests.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

[AttributeUsage(AttributeTargets.Class)]
public class SetTestNameAttribute : BeforeAfterTestAttribute
{
    private static readonly AsyncLocal<MethodInfo?> _local = new();

    /// <inheritdoc/>
    public override void Before(MethodInfo methodUnderTest)
    {
        _local.Value = methodUnderTest;
    }

    /// <inheritdoc/>
    public override void After(MethodInfo methodUnderTest)
    {
        _local.Value = null;
    }

    internal static bool TryGet([NotNullWhen(true)] out MethodInfo? info)
    {
        info = _local.Value;
        return info != null;
    }
}