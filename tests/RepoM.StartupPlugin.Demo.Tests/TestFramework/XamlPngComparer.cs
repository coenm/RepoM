namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VerifyTests;

internal static class XamlPngComparer
{
    private const int ChannelTolerance = 3;
    private const double MaximumChangedPixelRatio = 0.005;
    private const double MaximumMeanChannelError = 0.5;

    internal static Task<CompareResult> CompareAsync(
        Stream received,
        Stream verified,
        IReadOnlyDictionary<string, object> context)
    {
        BitmapSource receivedBitmap = Decode(received);
        BitmapSource verifiedBitmap = Decode(verified);
        return Task.FromResult(Compare(receivedBitmap, verifiedBitmap));
    }

    internal static CompareResult Compare(BitmapSource receivedBitmap, BitmapSource verifiedBitmap)
    {
        if (receivedBitmap.PixelWidth != verifiedBitmap.PixelWidth ||
            receivedBitmap.PixelHeight != verifiedBitmap.PixelHeight)
        {
            return CompareResult.NotEqual(
                $"Image dimensions differ: received {receivedBitmap.PixelWidth}x{receivedBitmap.PixelHeight}, " +
                $"verified {verifiedBitmap.PixelWidth}x{verifiedBitmap.PixelHeight}.");
        }

        receivedBitmap = new FormatConvertedBitmap(receivedBitmap, PixelFormats.Bgra32, null, 0);
        verifiedBitmap = new FormatConvertedBitmap(verifiedBitmap, PixelFormats.Bgra32, null, 0);
        int stride = receivedBitmap.PixelWidth * 4;
        int byteCount = stride * receivedBitmap.PixelHeight;
        var receivedPixels = new byte[byteCount];
        var verifiedPixels = new byte[byteCount];
        receivedBitmap.CopyPixels(receivedPixels, stride, 0);
        verifiedBitmap.CopyPixels(verifiedPixels, stride, 0);

        long totalChannelError = 0;
        int changedPixels = 0;
        int pixelCount = receivedBitmap.PixelWidth * receivedBitmap.PixelHeight;
        for (int offset = 0; offset < byteCount; offset += 4)
        {
            int maximumPixelError = 0;
            for (int channel = 0; channel < 4; channel++)
            {
                int difference = Math.Abs(receivedPixels[offset + channel] - verifiedPixels[offset + channel]);
                totalChannelError += difference;
                maximumPixelError = Math.Max(maximumPixelError, difference);
            }

            if (maximumPixelError > ChannelTolerance)
            {
                changedPixels++;
            }
        }

        double changedPixelRatio = (double)changedPixels / pixelCount;
        double meanChannelError = (double)totalChannelError / byteCount;
        if (changedPixelRatio <= MaximumChangedPixelRatio && meanChannelError <= MaximumMeanChannelError)
        {
            return CompareResult.Equal;
        }

        return CompareResult.NotEqual(
            $"Image difference exceeds tolerance: {changedPixelRatio:P3} of pixels differ by more than " +
            $"{ChannelTolerance} per channel; mean channel error is {meanChannelError:F3}.");
    }

    private static BitmapSource Decode(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        BitmapFrame frame = BitmapFrame.Create(
            stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return frame;
    }
}
