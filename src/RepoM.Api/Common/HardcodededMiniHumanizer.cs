namespace RepoM.Api.Common;

using System;
using System.Diagnostics.CodeAnalysis;
using RepoM.Core.Plugin.Common;

public class HardcodededMiniHumanizer : IHumanizer
{
    private readonly IClock _clock;

    public HardcodededMiniHumanizer(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public string HumanizeTimestamp(DateTime value)
    {
        TimeSpan diff = _clock.Now - value;

        if (TryHumanizeSpecials(diff, out var output))
        {
            return output;
        }

        if (TryHumanizeGeneric(diff, out output))
        {
            return output;
        }
        
        return value.ToString("g");
    }

    private static bool TryHumanizeGeneric(TimeSpan diff, [NotNullWhen(true)] out string? output)
    {
        output = null;
        var absoluteSeconds = Math.Abs(diff.TotalSeconds);
        var absoluteMinutes = Math.Abs(diff.TotalMinutes);
        var absoluteHours = Math.Abs(diff.TotalHours);
        var absoluteDays = Math.Abs(diff.TotalDays);

        if (absoluteSeconds < 60)
        {
            output = PastOrFuture($"{Math.Round(absoluteSeconds)} seconds", diff);
        }
        else if (absoluteMinutes < 60)
        {
            output = PastOrFuture($"{Math.Round(absoluteMinutes)} minutes", diff);
        }
        else if (absoluteHours < 24)
        {
            output = PastOrFuture($"{Math.Round(absoluteHours)} hours", diff);
        }
        else if (absoluteDays is >= 1.5 and < 5)
        {
            output = PastOrFuture($"{Math.Round(absoluteDays)} days", diff);
        }

        return output != null;

    }

    private static bool TryHumanizeSpecials(TimeSpan diff, [NotNullWhen(true)]out string? output)
    {
        output = null;
        var absoluteSeconds = Math.Abs(diff.TotalSeconds);
        var absoluteMinutes = Math.Abs(diff.TotalMinutes);
        var absoluteHours = Math.Abs(diff.TotalHours);

        if (absoluteSeconds < 25)
        {
            output = "Just now";
        }
        else if (absoluteSeconds is >= 55 and <= 80)
        {
            output = PastOrFuture("a minute", diff);
        }
        else if (absoluteSeconds is > 80 and <= 100)
        {
            output = PastOrFuture("nearly two minutes", diff);
        }
        else if (absoluteMinutes is >= 55 and <= 75)
        {
            output = PastOrFuture("an hour", diff);
        }
        else if (absoluteMinutes is > 75 and <= 100)
        {
            output = PastOrFuture("one and a half hour", diff);
        }
        else if (absoluteHours is >= 23 and <= 30)
        {
            output = PastOrFuture("a day", diff);
        }
        
        return output != null;
    }

    private static string PastOrFuture(string result, TimeSpan diff)
    {
        var value = diff.TotalMilliseconds > 0 ? result + " ago" : "in " + result;
        return value[..1].ToUpper() + value[1..];
    }
}