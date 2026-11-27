namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shipwreck.Phash;
using Shipwreck.Phash.Imaging;
using VerifyTests;

internal static class AnimatedGifComparer
{
    internal static Task<CompareResult> CompareAsync(
        Stream received,
        Stream verified,
        IReadOnlyDictionary<string, object> context)
    {
        return Task.FromResult(CompareGif(received, verified));
    }

    private static CompareResult CompareGif(
        Stream received,
        Stream verified)
    {
        if (received.CanSeek)
        {
            received.Position = 0;
        }

        if (verified.CanSeek)
        {
            verified.Position = 0;
        }

        var actual = new GifBitmapDecoder(received, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var expected = new GifBitmapDecoder(verified, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var actualMetadata = (BitmapMetadata)actual.Metadata;
        var expectedMetadata = (BitmapMetadata)expected.Metadata;
        int width = GetRequiredInt(actualMetadata, "/logscrdesc/Width");
        int height = GetRequiredInt(actualMetadata, "/logscrdesc/Height");
        int expectedWidth = GetRequiredInt(expectedMetadata, "/logscrdesc/Width");
        int expectedHeight = GetRequiredInt(expectedMetadata, "/logscrdesc/Height");
        if (width != expectedWidth || height != expectedHeight)
        {
            return CompareResult.NotEqual(
                $"GIF canvas dimensions differ: {width}x{height} and {expectedWidth}x{expectedHeight}.");
        }

        if (actual.Frames.Count != expected.Frames.Count)
        {
            return CompareResult.NotEqual($"GIF frame counts differ: {actual.Frames.Count} and {expected.Frames.Count}.");
        }

        if (!SameBytes(actualMetadata.GetQuery("/appext/Application"), expectedMetadata.GetQuery("/appext/Application")) ||
            !SameBytes(actualMetadata.GetQuery("/appext/Data"), expectedMetadata.GetQuery("/appext/Data")))
        {
            return CompareResult.NotEqual("GIF animation loop metadata differs.");
        }

        BitmapSource? actualCanvas = null;
        BitmapSource? expectedCanvas = null;
        for (var index = 0; index < actual.Frames.Count; index++)
        {
            BitmapFrame actualFrame = actual.Frames[index];
            BitmapFrame expectedFrame = expected.Frames[index];
            var actualFrameMetadata = (BitmapMetadata)actualFrame.Metadata;
            var expectedFrameMetadata = (BitmapMetadata)expectedFrame.Metadata;
            if (GetRequiredInt(actualFrameMetadata, "/grctlext/Delay") !=
                GetRequiredInt(expectedFrameMetadata, "/grctlext/Delay"))
            {
                return CompareResult.NotEqual($"GIF frame {index} delay differs.");
            }

            if (GetRequiredInt(actualFrameMetadata, "/grctlext/Disposal") is not (0 or 1) ||
                GetRequiredInt(expectedFrameMetadata, "/grctlext/Disposal") is not (0 or 1))
            {
                return CompareResult.NotEqual($"GIF frame {index} uses an unsupported disposal method.");
            }

            actualCanvas = DrawFrame(actualCanvas, actualFrame, actualFrameMetadata, width, height);
            expectedCanvas = DrawFrame(expectedCanvas, expectedFrame, expectedFrameMetadata, width, height);
            CompareResult result = CompareFrames(actualCanvas, expectedCanvas);
            if (!result.IsEqual)
            {
                return CompareResult.NotEqual($"GIF frame {index}: {result.Message}");
            }
        }

        return CompareResult.Equal;
    }

    private static CompareResult CompareFrames(BitmapSource actual, BitmapSource expected)
    {
        byte[] actualPixels = GetLuminancePixels(actual, out int width, out int height);
        byte[] expectedPixels = GetLuminancePixels(expected, out _, out _);
        // Uniform frames have no features for pHash to correlate, even when identical.
        if (actualPixels.AsSpan().SequenceEqual(expectedPixels))
        {
            return CompareResult.Equal;
        }

        bool matches = ImagePhash.CompareImages(
            new ByteImage(width, height, actualPixels),
            new ByteImage(width, height, expectedPixels),
            out float similarity,
            sigma: 3.5f,
            gamma: 1,
            numberOfAngles: 180,
            threshold: 0.80f);
        return matches
            ? CompareResult.Equal
            : CompareResult.NotEqual($"Perceptual similarity {similarity:F3} is below 0.800.");
    }

    private static byte[] GetLuminancePixels(BitmapSource image, out int width, out int height)
    {
        var scaled = new TransformedBitmap(image, new ScaleTransform(0.25, 0.25));
        var luminance = new FormatConvertedBitmap(scaled, PixelFormats.Gray8, null, 0);
        width = luminance.PixelWidth;
        height = luminance.PixelHeight;
        var pixels = new byte[width * height];
        luminance.CopyPixels(pixels, width, 0);
        return pixels;
    }

    private static bool SameBytes(object? left, object? right) =>
        left is byte[] leftBytes && right is byte[] rightBytes
            ? leftBytes.SequenceEqual(rightBytes)
            : left == null && right == null;

    private static int GetRequiredInt(BitmapMetadata metadata, string query) =>
        Convert.ToInt32(metadata.GetQuery(query) ?? throw new InvalidDataException($"GIF metadata '{query}' is missing."));

    private static BitmapSource DrawFrame(
        BitmapSource? canvas, BitmapFrame frame, BitmapMetadata metadata, int width, int height)
    {
        int left = GetRequiredInt(metadata, "/imgdesc/Left");
        int top = GetRequiredInt(metadata, "/imgdesc/Top");
        if (left < 0 || top < 0 || left + frame.PixelWidth > width || top + frame.PixelHeight > height)
        {
            throw new InvalidDataException("GIF frame extends beyond the canvas.");
        }

        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen())
        {
            if (canvas != null)
            {
                context.DrawImage(canvas, new Rect(0, 0, width, height));
            }

            context.DrawImage(frame, new Rect(left, top, frame.PixelWidth, frame.PixelHeight));
        }

        var result = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        result.Render(visual);
        result.Freeze();
        return result;
    }
}
