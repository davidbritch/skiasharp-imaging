namespace Imaging.Algorithms.FFT;

public class ComplexImage
{
    Complex[,] _data;
    int _width;
    int _height;
    bool _isFourierTransformed = false;

    #region Properties

    public Complex[,] Data
    {
        get => _data;
    }
    public int Width
    {
        get => _width;
    }
    public int Height
    {
        get => _height;
    }
    public bool IsFourierTransformed
    {
        get => _isFourierTransformed;
    }
    
    #endregion
    
    public ComplexImage(int w, int h)
    {
        _width = w;
        _height = h;
        _data = new Complex[_height, _width];
        _isFourierTransformed = false;
    }

    public void FastFourierTransform()
    {
        if (!_isFourierTransformed)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (((x + y) & 0x1) != 0)
                    {
                        _data[y, x].Re *= -1;
                        _data[y, x].Im *= -1;
                    }
                }
            }

            FourierTransform.FFT2D(_data, Direction.Forward);
            _isFourierTransformed = true;
        }
    }

    public void ReverseFastFourierTransform()
    {
        if (_isFourierTransformed)
        {
            FourierTransform.FFT2D(_data, Direction.Reverse);
            _isFourierTransformed = false;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (((x + y) & 0x1) != 0)
                    {
                        _data[y, x].Re *= -1;
                        _data[y, x].Im *= -1;
                    }
                }
            }
        }
    }
}