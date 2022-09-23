namespace RepoM.Api.Common;

using System;

public class HardcodededMiniHumanizer : IHumanizer
{
    private readonly IClock _clock;

    public HardcodededMiniHumanizer()
        : this(new SystemClock())
    {
    }

    public HardcodededMiniHumanizer(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public string HumanizeTimestamp(DateTime value)
    {
        TimeSpan diff = _clock.Now - value;

        var absoluteSeconds = Math.Abs(diff.TotalSeconds);
        var absoluteMinutes = Math.Abs(diff.TotalMinutes);
        var absoluteHours = Math.Abs(diff.TotalHours);
        var absoluteDays = Math.Abs(diff.TotalDays);

        // specials
        if (absoluteSeconds < 25)
        {
            return "Just now";
        }

        if (absoluteSeconds is >= 55 and <= 80)
        {
            return PastOrFuture("a minute", diff);
        }

        if (absoluteSeconds is > 80 and <= 100)
        {
            return PastOrFuture("nearly two minutes", diff);
        }

        if (absoluteMinutes is >= 55 and <= 75)
        {
            return PastOrFuture("an hour", diff);
        }

        if (absoluteMinutes is > 75 and <= 100)
        {
            return PastOrFuture("one and a half hour", diff);
        }

        if (absoluteHours is >= 23 and <= 30)
        {
            return PastOrFuture("a day", diff);
        }

        // generic
        if (absoluteSeconds < 60)
        {
            return PastOrFuture($"{Math.Round(absoluteSeconds)} seconds", diff);
        }

        if (absoluteMinutes < 60)
        {
            return PastOrFuture($"{Math.Round(absoluteMinutes)} minutes", diff);
        }

        if (absoluteHours < 24)
        {
            return PastOrFuture($"{Math.Round(absoluteHours)} hours", diff);
        }

        if (absoluteDays is >= 1.5 and < 5)
        {
            return PastOrFuture($"{Math.Round(absoluteDays)} days", diff);
        }

        // fallback
        return value.ToString("g");
    }

    private static string PastOrFuture(string result, TimeSpan diff)
    {
        var value = diff.TotalMilliseconds > 0 ? result + " ago" : "in " + result;
        return value.Substring(0, 1).ToUpper() + value.Substring(1);
    }
}