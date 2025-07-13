using SkiaSharp;

namespace Imaging.Services;

public interface IBitmapRendererService
{
    SKBitmap Bitmap { get; set; }
    SKPaint Paint { get; set; }
    SKPixmap Pixmap { get; }
    void PaintSurface(SKSurface surface, SKImageInfo info);
    void InvalidateSurface();
    event EventHandler InvalidateSurfaceRequest;
}