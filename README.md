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

The app's architected to use MVVM, with MVVM support coming from CommunityToolkit.Mvvm.
