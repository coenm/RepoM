namespace UiTests.Utils;

using System;
using System.Threading.Tasks;

public static class Delays
{
    public static TimeSpan DefaultKeyPressDelay { get; } = TimeSpan.FromMilliseconds(100);
    public static TimeSpan DefaultWaitUntilClick { get; } = TimeSpan.FromMilliseconds(1000);

    public static Task DelaySmallAsync()
    {
        return Task.Delay(100);
    }

    public static Task DelayMediumAsync()
    {
        return Task.Delay(1000);
    }
}