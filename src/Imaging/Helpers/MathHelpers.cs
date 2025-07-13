namespace Imaging.Helpers;

public static class MathHelpers
{
    public static bool IsPowerOf2(int x)
    {
        return (x > 0) ? ((x & (x - 1)) == 0) : false;
    }
}