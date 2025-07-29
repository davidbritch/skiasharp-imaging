namespace Imaging.Helpers;

public static class MathHelpers
{
    public static int LeftShift(int position)
    {
        return ((position >= 0) && (position <= 30)) ? (1 << position) : 0;
    }

    public static bool IsPowerOf2(int x)
    {
        return (x > 0) ? ((x & (x - 1)) == 0) : false;
    }
}    
