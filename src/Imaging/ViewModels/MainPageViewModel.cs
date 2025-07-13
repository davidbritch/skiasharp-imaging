using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using Imaging.Services;
using CommunityToolkit.Maui.Storage;
using Imaging.Extensions;
using Imaging.Algorithms;
using Imaging.Algoriths.Wavelets;

namespace Imaging.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    readonly IBitmapRendererService _bitmapService;
    readonly IFileSaver _fileSaver;
    SKPixmap _pixmap;
    string _fileName;

    #region Properties

    public IBitmapRendererService BitmapRenderer
    {
        get => _bitmapService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveImageCommand))]
    public string imageSaveFormat;

    [ObservableProperty]
    public double imageQuality = 75;

    [ObservableProperty]
    public string saveStatusMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PerformConvolutionCommand))]
    public string convolutionKernel;

    [ObservableProperty]
    public double minFrequency;

    [ObservableProperty]
    public double maxFrequency = 128;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(WaveletTransformImageCommand))]    
    public string waveletAlgorithm;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(WaveletTransformImageCommand))]
    public string selectedWavelet;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GreyscaleImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(ThresholdImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(SepiaImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PerformConvolutionCommand))]
    [NotifyCanExecuteChangedFor(nameof(FrequencyFilterImageCommand))]
    [NotifyCanExecuteChangedFor(nameof(WaveletTransformImageCommand))]
    public bool isLoaded;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveImageCommand))]
    public bool isDirty;

    #endregion

    public MainPageViewModel(IBitmapRendererService bitmapService, IFileSaver fileSaver)
    {
        _bitmapService = bitmapService;
        _fileSaver = fileSaver;
    }

    [RelayCommand]
    async Task LoadImage()
    {
        try
        {
            var image = await MediaPicker.PickPhotoAsync();
            await CopyImageAsync(image);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadImage threw: {ex.Message}");
        }
    }

    [RelayCommand]
    async Task CopyImageAsync(FileResult imageFile)
    {
        if (imageFile == null)
            return;

        // Save the file into local storage
        _fileName = imageFile.FileName;
        string localFilePath = Path.Combine(FileSystem.CacheDirectory, imageFile.FileName);
        using Stream sourceStream = await imageFile.OpenReadAsync();
        using FileStream localFileStream = File.OpenWrite(localFilePath);
        await sourceStream.CopyToAsync(localFileStream);

        sourceStream.Position = 0;
        _bitmapService.Bitmap = SKBitmap.Decode(sourceStream);
        IsLoaded = true;
    }

    SKEncodedImageFormat GetImageFormat()
    {
        SKEncodedImageFormat imageFormat;

        switch (ImageSaveFormat)
        {
            case "JPEG":
                imageFormat = SKEncodedImageFormat.Jpeg;
                break;
            case "PNG":
            default:
                imageFormat = SKEncodedImageFormat.Png;
                break;
        }
        return imageFormat;
    }

    [RelayCommand(CanExecute = nameof(CanSaveImage))]
    async Task SaveImage()
    {
        SKEncodedImageFormat imageFormat = GetImageFormat();
        int quality = (int)ImageQuality;
        string fileName = Path.ChangeExtension(_fileName, imageFormat.ToString().ToLower());

        if (_pixmap == null)
            _pixmap = _bitmapService.Pixmap;

        using MemoryStream memStream = new MemoryStream();
        bool result = _pixmap.Encode(memStream, imageFormat, quality);

        try
        {
            var fileLocationResult = await _fileSaver.SaveAsync(fileName, memStream);
            fileLocationResult.EnsureSuccess();
            SaveStatusMessage = $"File saved: {fileLocationResult.FilePath}";
            IsDirty = false;
        }
        catch (Exception ex)
        {
            SaveStatusMessage = $"File isn't saved: {ex.Message}";
        }
    }

    bool CanSaveImage()
    {
        return IsDirty && !string.IsNullOrWhiteSpace(ImageSaveFormat);
    }

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    void GreyscaleImage()
    {
        _pixmap = _bitmapService.Bitmap.ToGreyscale();
        _bitmapService.InvalidateSurface();
        IsDirty = true;
    }

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    void ThresholdImage()
    {
        _pixmap = _bitmapService.Bitmap.OtsuThreshold();
        _bitmapService.InvalidateSurface();
        IsDirty = true;
    }

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    void SepiaImage()
    {
        _pixmap = _bitmapService.Bitmap.ToSepia();
        _bitmapService.InvalidateSurface();
        IsDirty = true;
    }

    float[] GetConvolutionKernel()
    {
        float[] kernel = null;

        switch (ConvolutionKernel)
        {
            case "Box Blur":
                kernel = ConvolutionKernels.BoxBlur;
                break;

            case "Blur":
                kernel = ConvolutionKernels.Blur;
                break;

            case "Edge Detection":
                kernel = ConvolutionKernels.EdgeDetection;
                break;

            case "Emboss":
                kernel = ConvolutionKernels.Emboss;
                break;

            case "Gaussian Blur":
                kernel = ConvolutionKernels.GaussianBlur;
                break;

            case "Identity":
                kernel = ConvolutionKernels.Identity;
                break;

            case "Laplacian Of Gaussian":
                kernel = ConvolutionKernels.LaplacianOfGaussian;
                break;

            case "Sharpen":
                kernel = ConvolutionKernels.Sharpen;
                break;

            case "Sobel Bottom":
                kernel = ConvolutionKernels.SobelBottom;
                break;

            case "Sobel Left":
                kernel = ConvolutionKernels.SobelLeft;
                break;

            case "Sobel Right":
                kernel = ConvolutionKernels.SobelRight;
                break;

            case "Sobel Top":
                kernel = ConvolutionKernels.SobelTop;
                break;
        }
        return kernel;
    }

    [RelayCommand(CanExecute = nameof(CanPerformConvolution))]
    void PerformConvolution()
    {
        float[] kernel = GetConvolutionKernel();

        int length = kernel.Length;
        int size = (int)Math.Sqrt(length);
        SKSizeI sizeI = new SKSizeI(size, size);

        SKPaint paint = new SKPaint()
        {
            IsAntialias = false,
            IsDither = false
        };
        paint.ImageFilter = SKImageFilter.CreateMatrixConvolution(sizeI, kernel, 1f, 0f, new SKPointI(1, 1), SKShaderTileMode.Clamp, false);

        _bitmapService.Paint = paint;
        _bitmapService.InvalidateSurface();
        IsDirty = true;
        _pixmap = null;
    }

    bool CanPerformConvolution()
    {
        return IsLoaded && !string.IsNullOrWhiteSpace(ConvolutionKernel);
    }

    [RelayCommand(CanExecute = nameof(IsLoaded))]
    void FrequencyFilterImage()
    {
        _pixmap = _bitmapService.Bitmap.FrequencyFilter((int)MinFrequency, (int)MaxFrequency);
        _bitmapService.InvalidateSurface();
        IsDirty = true;
    }

    [RelayCommand(CanExecute = nameof(CanWaveletTransformImage))]
    void WaveletTransformImage()
    {
        WaveletOperation operation = WaveletAlgorithm == WaveletOperation.AdaptiveSharpening.ToString() ? WaveletOperation.AdaptiveSharpening : WaveletOperation.ImageUpscaling;
        Wavelet wavelet = SelectedWavelet == Wavelet.Biorthogonal53.ToString() ? Wavelet.Biorthogonal53 : Wavelet.Haar;

        if (operation == WaveletOperation.AdaptiveSharpening)
        {
            _pixmap = _bitmapService.Bitmap.WaveletAdaptiveSharpening(wavelet);
            _bitmapService.InvalidateSurface();
            IsDirty = true;
        }
        else
        {
            _pixmap = _bitmapService.Bitmap.WaveletUpscale(wavelet);
            _bitmapService.InvalidateSurface();
            IsDirty = true;
        }
    }

    bool CanWaveletTransformImage()
    {
        return IsLoaded && !string.IsNullOrWhiteSpace(WaveletAlgorithm) && !string.IsNullOrWhiteSpace(SelectedWavelet) ;
    }
}
