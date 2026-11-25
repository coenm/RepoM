namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AwesomeAssertions;
using VerifyTests;
using Xunit;

public class AnimatedGifComparerTests
{
    [WpfFact]
    public async Task Compare_ShouldApplyPixelToleranceToEveryCompositedFrame()
    {
        byte[] expected = CreateGif(changedPixels: 0);

        CompareResult withinTolerance = await CompareAsync(CreateGif(changedPixels: 5), expected);
        CompareResult outsideTolerance = await CompareAsync(CreateGif(changedPixels: 6), expected);

        withinTolerance.IsEqual.Should().BeTrue();
        outsideTolerance.IsEqual.Should().BeFalse();
        outsideTolerance.Message.Should().Contain("frame 1");
    }

    [WpfFact]
    public async Task Compare_ShouldAcceptDifferentFrameRegionsWithTheSameVisualResult()
    {
        byte[] fullFrame = CreateGif(changedPixels: 5);
        byte[] croppedFrame = CreateGif(changedPixels: 5, cropped: true);

        fullFrame.Should().NotEqual(croppedFrame);
        (await CompareAsync(fullFrame, croppedFrame)).IsEqual.Should().BeTrue();
    }

    [WpfFact]
    public async Task Compare_ShouldRejectDifferentFrameDelays()
    {
        CompareResult result = await CompareAsync(CreateGif(0, delay: 5), CreateGif(0, delay: 4));

        result.IsEqual.Should().BeFalse();
        result.Message.Should().Contain("delay");
    }

    [WpfFact]
    public async Task Compare_ShouldRejectDifferentCanvasSizesAndFrameCounts()
    {
        byte[] expected = CreateGif(0);

        CompareResult differentSize = await CompareAsync(CreateGif(0, size: 33), expected);
        CompareResult differentCount = await CompareAsync(CreateGif(0, twoFrames: false), expected);

        differentSize.IsEqual.Should().BeFalse();
        differentSize.Message.Should().Contain("dimensions");
        differentCount.IsEqual.Should().BeFalse();
        differentCount.Message.Should().Contain("frame counts");
    }

    private static async Task<CompareResult> CompareAsync(byte[] received, byte[] verified)
    {
        using var receivedStream = new MemoryStream(received);
        using var verifiedStream = new MemoryStream(verified);
        return await AnimatedGifComparer.CompareAsync(receivedStream, verifiedStream, new Dictionary<string, object>());
    }

    private static byte[] CreateGif(int changedPixels, bool cropped = false, ushort delay = 4, int size = 32, bool twoFrames = true)
    {
        var pixels = new byte[size * size * 4];
        for (var offset = 0; offset < pixels.Length; offset += 4)
        {
            pixels[offset] = 100;
            pixels[offset + 1] = 100;
            pixels[offset + 2] = 100;
            pixels[offset + 3] = 255;
        }

        BitmapSource first = BitmapSource.Create(size, size, 96, 96, PixelFormats.Bgra32, null, pixels, size * 4);
        if (!twoFrames)
        {
            return AnimatedGifWriter.Write(size, size, [(first, new Int32Rect(0, 0, size, size), 4)]);
        }

        for (var index = 0; index < changedPixels; index++)
        {
            pixels[(((8 * size) + 8 + index) * 4) + 2] = 140;
        }

        BitmapSource second = BitmapSource.Create(size, size, 96, 96, PixelFormats.Bgra32, null, pixels, size * 4);
        return AnimatedGifWriter.Write(size, size,
        [
            (first, new Int32Rect(0, 0, size, size), 4),
            (second, cropped ? new Int32Rect(8, 8, 8, 8) : new Int32Rect(0, 0, size, size), delay),
        ]);
    }
}
