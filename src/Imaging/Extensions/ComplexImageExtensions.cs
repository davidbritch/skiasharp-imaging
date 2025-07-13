using SkiaSharp;
using Imaging.Algorithms.FFT;

namespace Imaging.Extensions;

public static class ComplexImageExtensions
{
    public static unsafe SKPixmap ToSKPixmap(this ComplexImage complexImage, SKBitmap bitmap)
    {
        SKPixmap pixmap = bitmap.PeekPixels();
        byte* bmpPtr = (byte*)pixmap.GetPixels().ToPointer();
        int width = bitmap.Width;
        int height = bitmap.Height;

        Complex[,] data = complexImage.Data;
        double scale = (complexImage.IsFourierTransformed) ? Math.Sqrt(width * height) : 1;

        bmpPtr = (byte*)pixmap.GetPixels().ToPointer();
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                // Assuming SKColorType.Rgba8888 - used by Apple platforms and Android
                byte result = (byte)Math.Max(0, Math.Min(255, data[row, col].Magnitude * scale * 255));
                *bmpPtr++ = result; // red
                *bmpPtr++ = result; // green
                *bmpPtr++ = result; // blue
                bmpPtr += 1; // Ignore alpha
            }
        }

        return pixmap;
    }
}
