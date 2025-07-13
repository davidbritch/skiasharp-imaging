namespace Imaging.Algoriths.Wavelets;

public class OrderWavelet2D : WaveletTransform2D
{
    protected const int AllowedMinSize = 2;

    public OrderWavelet2D(int width, int height)
        : base(AllowedMinSize, AllowedMinSize, width, height)
    {
    }

    public OrderWavelet2D(int width, int height, int minSize)
        : base(minSize, AllowedMinSize, width, height)
    {
    }

    protected override void TransformRow(float[,] source, float[,] dest, int y, int length)
    {
        if (length >= AllowedMinSize)
        {
            int half = length >> 1;
            int offSrc = 0;

            int numLFValues = half + (length & 1);

            for (int i = 0; i < half; i++)
            {
                dest[i, y] = source[offSrc, y];
                dest[i + numLFValues, y] = source[offSrc + 1, y];
                offSrc += 2;
            }
            if ((length & 1) != 0)
                dest[numLFValues - 1, y] = source[length - 1, y];
        }
        else
        {
            for (int i = 0; i < length; i++)
                dest[i, y] = source[i, y];
        }
    }

    protected override void TransformCol(float[,] source, float[,] dest, int x, int length)
    {
        if (length >= AllowedMinSize)
        {
            int half = length >> 1;
            int offSrc = 0;

            int numLFValues = half + (length & 1);

            for (int i = 0; i < half; i++)
            {
                dest[x, i] = source[x, offSrc];
                dest[x, i + numLFValues] = source[x, offSrc + 1];
                offSrc += 2;
            }
            if ((length & 1) != 0)
                dest[x, numLFValues - 1] = source[x, length - 1];
        }
        else
        {
            for (int i = 0; i < length; i++)
                dest[x, i] = source[x, i];
        }
    }

    protected override void InverseTransformRow(float[,] source, float[,] dest, int y, int length)
    {
        if (length >= AllowedMinSize)
        {
            int half = length >> 1;
            int offDst = 0;

            int numLFValues = half + (length & 1);

            for (int i = 0; i < half; i++) {
                dest[offDst, y] = source[i, y];
                dest[offDst + 1, y] = source[i + numLFValues, y];
                offDst += 2;
            }							
            if ((length & 1) != 0)
                dest[length - 1, y] = source[numLFValues - 1, y]; 
        }
        else
        {
            for (int i = 0; i < length; i++)
                dest[i, y] = source[i, y];
        }
    }

    protected override void InverseTransformCol(float[,] source, float[,] dest, int x, int length)
    {
        if (length >= AllowedMinSize)
        {
            int half = length >> 1;
            int offDst = 0;

            int numLFValues = half + (length & 1);

            for (int i = 0; i < half; i++) {
                dest[x, offDst] = source[x, i];
                dest[x, offDst + 1] = source[x, i + numLFValues];
                offDst += 2;
            }							
            if ((length & 1) != 0)
                dest[x, length - 1] = source[x, numLFValues - 1]; 
        }
        else
        {
            for (int i = 0; i < length; i++)
                dest[x, i] = source[x, i];
        }
    }
}