namespace Imaging.Algorithms.FFT;

public struct Complex
{
    public double Re; // Real
    public double Im; // Imaginary

    public double Magnitude
    {
        get => Math.Sqrt(Re * Re + Im * Im);
    }

    public double Phase
    {
        get => Math.Atan2(Im, Re);
    }

    public Complex(double re, double im)
    {
        Re = re;
        Im = im;
    }

    public Complex(Complex c)
    {
        Re = c.Re;
        Im = c.Im;
    }
}