using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;
using Imaging.Services;

namespace Imaging.Controls;

public class MySKCanvasView : SKCanvasView
{
    public static readonly BindableProperty CanvasRendererProperty =
        BindableProperty.Create(nameof(CanvasRenderer), typeof(IBitmapRendererService), typeof(MySKCanvasView), null, defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((MySKCanvasView)bindable).CanvasRendererChanged((IBitmapRendererService)oldValue, (IBitmapRendererService)newValue);
        });

    public IBitmapRendererService CanvasRenderer
    {
        get { return (IBitmapRendererService)GetValue(CanvasRendererProperty); }
        set { SetValue(CanvasRendererProperty, value); }
    }

    void CanvasRendererChanged(IBitmapRendererService currentRenderer, IBitmapRendererService newRenderer)
    {
        if (currentRenderer != newRenderer)
        {
            if (currentRenderer != null)
                currentRenderer.InvalidateSurfaceRequest -= CanvasRendererInvalidateSurfaceRequest;

            if (newRenderer != null)
                newRenderer.InvalidateSurfaceRequest += CanvasRendererInvalidateSurfaceRequest;

            InvalidateSurface();
        }
    }

    void CanvasRendererInvalidateSurfaceRequest(object sender, EventArgs e)
    {
        InvalidateSurface();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        CanvasRenderer.PaintSurface(e.Surface, e.Info);
    }
}
