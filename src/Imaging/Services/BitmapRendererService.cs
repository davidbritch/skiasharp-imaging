using SkiaSharp;
using Imaging.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Imaging.Services;

public class BitmapRendererService : ObservableObject, IBitmapRendererService
{

    #region Properties

    SKBitmap _bitmap = null;
    public SKBitmap Bitmap
    {
        get => _bitmap;
        set
        {
            SetProperty(ref _bitmap, value);
            InvalidateSurfaceRequest?.Invoke(this, EventArgs.Empty);
        }
    }

    SKPaint _paint = null;
    public SKPaint Paint
    {
        get => _paint;
        set { SetProperty(ref _paint, value); }
    }

    SKPixmap _pixmap = null;
    public SKPixmap Pixmap
    {
        get => _pixmap;
    }

    #endregion

    public void PaintSurface(SKSurface surface, SKImageInfo info)
    {
        SKCanvas canvas = surface.Canvas;
        canvas.Clear();

        if (_bitmap != null && _paint != null)
        {
            // Used for drawing the result of convolution operations
            canvas.DrawBitmap(_bitmap, info.Rect, ImageStretch.Uniform, paint: _paint);
            SKImage image = surface.Snapshot(); // This isn't ideal because the new image will be the size of the surface, rather than the original dimensions
            _bitmap = SKBitmap.FromImage(image);
            _pixmap = _bitmap.PeekPixels();
            _paint = null;
        }
        else if (_bitmap != null && _paint == null)
        {
            // Used for everything but convolution
            canvas.DrawBitmap(_bitmap, info.Rect, ImageStretch.Uniform);
            _pixmap = null;
        }
    }

    public void InvalidateSurface()
    {
        InvalidateSurfaceRequest?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler InvalidateSurfaceRequest;
}