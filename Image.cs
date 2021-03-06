using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class Image
{
    public static uint _5551to8888(ushort input) =>
    (uint) (
        (input & 1) * 0xff << 24 |
        (input & 0x3e)     << 2  |
        (input & 0x7c0)    << 5  |
        (input & 0xf800)   << 8
    );

    public static uint IA8To8888(ushort input)
    {
        var alpha = input & 0xff;
        var grey = input >> 8;
        return (uint)(grey | (grey << 8) | (grey << 16) | (alpha << 24));
    }

    public static void SaveImage(byte[] buf, string baseDir, bool isThumb)
    {
        var bmp = isThumb ? new Bitmap(56, 56) : new Bitmap(184, 24);
        var img = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        Marshal.Copy(buf, 0, img.Scan0, buf.Length);
        bmp.UnlockBits(img);
        bmp.Save($"{baseDir}/{(isThumb ? "thumb.png" : "title.png")}", ImageFormat.Png);
    }
}
