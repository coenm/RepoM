namespace RepoM.StartupPlugin.Demo.Tests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RepoM.Api.Git;
using RepoM.StartupPlugin.Demo.Tests.TestFramework;
using Xunit;

public partial class DemoContextMenuSnapshotTests
{
    private const int DesktopWidth = 1920;
    private const int DesktopHeight = 1080;
    private const int TaskbarHeight = 48;
    private const string OpenShortcut = "Ctrl + Alt + R";

    [Fact]
    public Task RepositoryFilter_ShouldMatchVideoSnapshot() =>
        _sta.RunAsync(RepositoryFilter_ShouldMatchVideoSnapshotOnStaAsync);

    private async Task RepositoryFilter_ShouldMatchVideoSnapshotOnStaAsync()
    {
        await using var session = await DemoSession.StartAsync("Dark");
        session.ParkRealMouse();
        session.ClearFilterFocus();
        IReadOnlyList<Layer> idle = await session.CaptureStableLayersAsync(expectContextMenu: false);
        Point filter = session.FilterPoint;
        Point start = new(filter.X - 150, filter.Y + 100);
        var frames = new List<VideoFrame>
        {
            new([], start, null, 80),
            new([], start, OpenShortcut, 65),
        };
        int revealFrame = frames.Count;
        frames.Add(new(idle, start, null, 80));

        AddMove(frames, idle, start, filter);
        frames.Add(new(idle, filter, "Click", 40));
        session.ClickFilter();
        await WaitUntilAsync(
            () => session.Window.IsActive && session.FilterBox.IsKeyboardFocused,
            "The filter box did not receive keyboard focus.");
        session.FilterBox.Clear();
        session.ParkRealMouse();

        string typedText = string.Empty;
        foreach (char character in "web")
        {
            typedText += character;
            session.TypeText(character.ToString());
            session.ClearFilterFocus();
            await WaitUntilAsync(
                () => session.FilterBox.Text == typedText && session.List.Items.Count == 1 &&
                    !session.FilterBox.IsKeyboardFocused,
                $"The repository list did not filter to one result for '{typedText}'. " +
                $"Text was '{session.FilterBox.Text}', window active: {session.Window.IsActive}, " +
                $"filter focused: {session.FilterBox.IsKeyboardFocused}, items: {session.List.Items.Count}.");

            IReadOnlyList<Layer> filtered = await session.CaptureStableLayersAsync(expectContextMenu: false);
            frames.Add(new(filtered, filter, char.ToUpperInvariant(character).ToString(), 55));
        }

        if (session.List.Items[0] is not RepositoryViewModel repository || repository.Name != "WebShop")
        {
            throw new InvalidOperationException("Typing 'web' should leave only the WebShop repository visible.");
        }

        Rect canvas = GetBounds(frames.SelectMany(frame => frame.Layers).ToList());
        var gifFrames = new List<(BitmapSource Image, Int32Rect Region, ushort DelayCentiseconds)>();
        byte[]? previous = null;
        foreach (VideoFrame frame in frames)
        {
            BitmapSource? appImage = frame.Layers.Count == 0
                ? null
                : Compose(frame.Layers, canvas, _videoBackground, frame.Cursor);
            RenderTargetBitmap image = ComposeFakeDesktop(appImage, frame.KeyLabel);
            int stride = image.PixelWidth * 4;
            byte[] pixels = new byte[stride * image.PixelHeight];
            image.CopyPixels(pixels, stride, 0);
            Int32Rect changed = previous == null
                ? new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight)
                : GetChangedRegion(previous, pixels, image.PixelWidth, image.PixelHeight);
            previous = pixels;
            gifFrames.Add((image, changed, frame.Delay));
        }

        var palette = new BitmapPalette(gifFrames[revealFrame].Image, 256);
        byte[] gif = AnimatedGifWriter.Write(DesktopWidth, DesktopHeight, gifFrames, palette);
        await VerifyImageAsync("gif", gif, "Filter.Dark");
    }

    private static RenderTargetBitmap ComposeFakeDesktop(BitmapSource? appImage, string? actionLabel)
    {
        const double windowBorder = 2;
        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen())
        {
            var wallpaper = new LinearGradientBrush(
                Color.FromRgb(0x10, 0x32, 0x50),
                Color.FromRgb(0x36, 0x65, 0x77),
                35);
            context.DrawRectangle(wallpaper, null, new Rect(0, 0, DesktopWidth, DesktopHeight));

            DrawComputerIcon(context);

            if (appImage != null)
            {
                double imageWidth = appImage.PixelWidth;
                double imageHeight = appImage.PixelHeight;
                double windowLeft = DesktopWidth - imageWidth - windowBorder;
                double windowTop = DesktopHeight - TaskbarHeight - imageHeight - windowBorder;
                var windowBounds = new Rect(
                    windowLeft - windowBorder, windowTop - windowBorder,
                    imageWidth + 2 * windowBorder, imageHeight + 2 * windowBorder);
                context.DrawRoundedRectangle(
                    new SolidColorBrush(Color.FromArgb(220, 12, 16, 22)),
                    null,
                    windowBounds,
                    7,
                    7);
                context.DrawImage(appImage, new Rect(windowLeft, windowTop, imageWidth, imageHeight));
            }

            if (actionLabel != null)
            {
                bool isClick = actionLabel == "Click";
                bool isShortcut = actionLabel == OpenShortcut;
                double badgeWidth = isShortcut || isClick ? 148 : 62;
                var badge = new Rect(
                    (DesktopWidth - badgeWidth) / 2,
                    DesktopHeight - TaskbarHeight - 60,
                    badgeWidth,
                    44);
                context.DrawRoundedRectangle(
                    new SolidColorBrush(Color.FromRgb(0x1B, 0x20, 0x28)),
                    new Pen(new SolidColorBrush(Color.FromRgb(0x87, 0xA8, 0xC5)), 1),
                    badge,
                    7,
                    7);
                DrawText(
                    context,
                    isClick ? "MOUSE CLICK" : isShortcut ? actionLabel : actionLabel.ToUpperInvariant(),
                    new Point(badge.X + (isShortcut ? badge.Width / 2 : isClick ? 17 : 20), badge.Y + 12),
                    isClick ? 13 : 17,
                    Brushes.White,
                    isShortcut ? TextAlignment.Center : TextAlignment.Left);
            }

            DrawTaskbar(context);
        }

        var bitmap = new RenderTargetBitmap(DesktopWidth, DesktopHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private static void DrawComputerIcon(DrawingContext context)
    {
        const double centerX = 51;
        var metal = new SolidColorBrush(Color.FromRgb(0xD5, 0xE2, 0xED));
        context.DrawRoundedRectangle(metal, null, new Rect(30, 32, 42, 29), 3, 3);
        context.DrawRoundedRectangle(
            new SolidColorBrush(Color.FromRgb(0x30, 0x81, 0xCE)), null, new Rect(33, 35, 36, 22), 2, 2);
        context.DrawRectangle(metal, null, new Rect(48, 61, 6, 6));
        context.DrawRoundedRectangle(metal, null, new Rect(39, 67, 24, 4), 2, 2);
        DrawText(context, "My Computer", new Point(centerX, 77), 12, Brushes.White, TextAlignment.Center);
    }

    private static void DrawTaskbar(DrawingContext context)
    {
        double top = DesktopHeight - TaskbarHeight;
        context.DrawRectangle(
            new SolidColorBrush(Color.FromArgb(245, 24, 28, 36)),
            null,
            new Rect(0, top, DesktopWidth, TaskbarHeight));

        var startIcon = new SolidColorBrush(Color.FromRgb(0x55, 0xA9, 0xF5));
        for (var row = 0; row < 2; row++)
        {
            for (var column = 0; column < 2; column++)
            {
                context.DrawRectangle(startIcon, null, new Rect(24 + (column * 10), top + 14 + (row * 10), 8, 8));
            }
        }

        var search = new Rect(58, top + 8, 170, 32);
        context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(0x36, 0x3B, 0x45)), null, search, 6, 6);
        DrawText(context, "Search", new Point(search.X + 33, search.Y + 7), 13, Brushes.White);

        Color[] iconColors = [
            Color.FromRgb(0x3B, 0x82, 0xC4),
            Color.FromRgb(0xC5, 0x68, 0x4B),
            Color.FromRgb(0x56, 0x9B, 0x6A),
            Color.FromRgb(0x93, 0x70, 0xC6),
        ];
        for (var index = 0; index < iconColors.Length; index++)
        {
            context.DrawRoundedRectangle(
                new SolidColorBrush(iconColors[index]),
                null,
                new Rect(DesktopWidth / 2 - 160 + (index * 46), top + 11, 26, 26),
                5,
                5);
        }

        DrawText(context, "10:09", new Point(DesktopWidth - 90, top + 7), 12, Brushes.White);
        DrawText(context, "Fri 27 Nov", new Point(DesktopWidth - 110, top + 25), 10, Brushes.White);
    }

    private static void DrawText(
        DrawingContext context, string text, Point origin, double size, Brush brush, TextAlignment alignment = TextAlignment.Left)
    {
        var formattedText = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            size,
            brush,
            1)
        {
            TextAlignment = alignment,
        };
        context.DrawText(formattedText, origin);
    }

}
