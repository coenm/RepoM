namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// <summary>
/// Writes an endlessly looping animated GIF where each frame may cover only part of the canvas.
/// WPF's <see cref="GifBitmapEncoder"/> ignores frame delays and offsets, so it is only used to compress single frames.
/// </summary>
internal static class AnimatedGifWriter
{
    public static byte[] Write(
        int width,
        int height,
        IEnumerable<(BitmapSource Image, Int32Rect Region, ushort DelayCentiseconds)> frames,
        BitmapPalette? palette = null)
    {
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        writer.Write(Encoding.ASCII.GetBytes("GIF89a"));
        writer.Write((ushort)width);
        writer.Write((ushort)height);
        writer.Write([(byte)0, (byte)0, (byte)0]);
        writer.Write([(byte)0x21, (byte)0xFF, (byte)0x0B]);
        writer.Write(Encoding.ASCII.GetBytes("NETSCAPE2.0"));
        writer.Write([(byte)0x03, (byte)0x01, (byte)0x00, (byte)0x00, (byte)0x00]);

        foreach (var (image, region, delay) in frames)
        {
            BitmapSource cropped = new CroppedBitmap(image, region);
            var indexed = new FormatConvertedBitmap(cropped, PixelFormats.Indexed8, palette ?? new BitmapPalette(cropped, 256), 0);
            var (colorTable, colorTableSize, imageData) = EncodeSingleFrame(indexed);

            // Graphic control extension: keep the previous frame in place and wait before showing the next one.
            writer.Write([(byte)0x21, (byte)0xF9, (byte)0x04, (byte)0x04]);
            writer.Write(delay);
            writer.Write([(byte)0x00, (byte)0x00]);

            writer.Write((byte)0x2C);
            writer.Write((ushort)region.X);
            writer.Write((ushort)region.Y);
            writer.Write((ushort)region.Width);
            writer.Write((ushort)region.Height);
            writer.Write((byte)(0x80 | colorTableSize));
            writer.Write(colorTable);
            writer.Write(imageData);
        }

        writer.Write((byte)0x3B);
        writer.Flush();
        return output.ToArray();
    }

    private static (byte[] ColorTable, int ColorTableSize, byte[] ImageData) EncodeSingleFrame(BitmapSource frame)
    {
        var encoder = new GifBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(frame));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        byte[] gif = stream.ToArray();

        byte[]? colorTable = null;
        int colorTableSize = 0;
        int position = 13;
        if ((gif[10] & 0x80) != 0)
        {
            colorTableSize = gif[10] & 0x07;
            colorTable = gif.AsSpan(position, ColorTableLength(colorTableSize)).ToArray();
            position += colorTable.Length;
        }

        while (gif[position] == 0x21)
        {
            position = SkipSubBlocks(gif, position + 2);
        }

        if (gif[position] != 0x2C)
        {
            throw new InvalidDataException("The encoded GIF frame has no image descriptor.");
        }

        byte packed = gif[position + 9];
        position += 10;
        if ((packed & 0x80) != 0)
        {
            colorTableSize = packed & 0x07;
            colorTable = gif.AsSpan(position, ColorTableLength(colorTableSize)).ToArray();
            position += colorTable.Length;
        }

        if (colorTable == null)
        {
            throw new InvalidDataException("The encoded GIF frame has no color table.");
        }

        int dataEnd = SkipSubBlocks(gif, position + 1);
        return (colorTable, colorTableSize, gif.AsSpan(position, dataEnd - position).ToArray());
    }

    private static int ColorTableLength(int size) => 3 * (1 << (size + 1));

    private static int SkipSubBlocks(byte[] gif, int position)
    {
        while (gif[position] != 0)
        {
            position += gif[position] + 1;
        }

        return position + 1;
    }
}
