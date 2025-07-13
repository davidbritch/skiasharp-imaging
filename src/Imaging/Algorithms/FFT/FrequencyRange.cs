namespace Imaging.Algorithms.FFT;

public class FrequencyRange
{
    int _min;
    int _max;

    public int Min
    {
        get => _min;
        set => _min = value;
    }

    public int Max
    {
        get => _max;
        set => _max = value;
    }

    public FrequencyRange(int min, int max)
    {
        _min = min;
        _max = max;
    }
}