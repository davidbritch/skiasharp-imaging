# 2D imaging using SkiaSharp

This repo contains a prototype that shows how to perform 2D image processing with SkiaSharp, hosted in a .NET MAUI app that targets Mac Catalyst. With a little work the app could also target iOS and Android. Targeting Windows requires more work due to Windows using a different pixel format.

The app performs the following operations:

- Loads images from the photo library.
- Saves modified images (as PNG/JPEG) to the file system.
- Converts images to greyscale.
- Converts images to serpia.
- Thresholds images using Otsu's algorithm.
- Performs convolution using different selectable convolution kernels (blur, emboss, edge detection etc).
- Performs frequency filtering using a Fast Fourier Transform.
- Performs image upscaling and adaptive sharpening using an asymmetric wavelet transform.

With the exception of the convolution operations, all other algorithms manipulate image data at the pixel level.

The app uses the MVVM pattern, with MVVM support coming from CommunityToolkit.Mvvm.

For more information, see the following blog posts:

- [SkiaSharp and MVVM](https://davestechlab.co.uk/skiasharp-and-mvvm/)
- [Getting pixel data with SkiaSharp](https://davestechlab.co.uk/getting-pixel-data-with-skiasharp/)
- [Performing convolution in SkiaSharp](https://davestechlab.co.uk/performing-convolution-in-skiasharp/)
- [Frequency filtering in SkiaSharp](https://davestechlab.co.uk/frequency-filtering-in-skiasharp/)
- [Wavelet transforms in SkiaSharp](https://davestechlab.co.uk/wavelet-transforms-in-skiasharp/)
