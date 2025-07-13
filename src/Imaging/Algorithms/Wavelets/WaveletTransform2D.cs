using AuthenticationServices;

namespace Imaging.Algoriths.Wavelets;

public abstract class WaveletTransform2D
{
    protected int _width;
    protected int _height;
    protected int _minSize;
    protected int _allowedMinSize;

    volatile bool _enableParallel = true;
    volatile bool _enableCaching = false;
    volatile float[,] _cachedArray = null;
    object _threadSync = new object();

    public bool EnableParallel
    {
        get { return _enableParallel; }
        set { _enableParallel = value; }
    }

    public bool EnableCaching
    {
        get { return _enableCaching; }
        set
        {
            _enableCaching = value;
            if (!value)
                FlushCache();
        }
    }

    public WaveletTransform2D(int minSize, int allowedMinSize, int width, int height)
    {
        if (allowedMinSize < 1)
            throw new ArgumentException("allowedMinSize can't be less than one");
        if (minSize < allowedMinSize)
            throw new ArgumentException("minSize can't be smaller than " + allowedMinSize);
        if (width < minSize || height < minSize)
            throw new ArgumentException("width and height must be greater or equal to " + minSize);

        _width = width;
        _height = height;
        _minSize = minSize;
        _allowedMinSize = allowedMinSize;
    }

    void FlushCache()
    {
        lock (_threadSync)
            _cachedArray = null;
    }

    protected float[,] GetTempArray()
    {
        float[,] temp = _cachedArray;

        if (temp == null)
        {
            temp = new float[_width, _height];
            if (_enableCaching)
                _cachedArray = temp;
        }

        return temp;
    }

    void CheckArrayArgument(float[,] source, string name)
    {
        if (source == null)
            throw new ArgumentException(name + " can't be null");
        if (source.GetLength(0) < _width)
            throw new ArgumentException("first dimension of " + name + " can't be smaller than " + _width);
        if (source.GetLength(1) < _height)
            throw new ArgumentException("second dimension of " + name + " can't be smaller than " + _height);
    }

    virtual protected void TransformRow(float[,] source, float[,] dest, int y, int length)
    {
    }

    virtual protected void TransformCol(float[,] source, float[,] dest, int x, int length)
    {
    }

    virtual protected void InverseTransformRow(float[,] source, float[,] dest, int y, int length)
    {
    }

    virtual protected void InverseTransformCol(float[,] source, float[,] dest, int x, int length)
    {
    }

    virtual protected void TransformRows(float[,] source, float[,] dest, int w, int h)
    {
        if (_enableParallel)
        {
            Parallel.For(0, h, (y, loopState) =>
            {
                TransformRow(source, dest, y, w);
            });
        }
        else
        {
            for (int y = 0; y < h; y++)
            {
                TransformRow(source, dest, y, w);
            }
        }
    }

    virtual protected void TransformCols(float[,] source, float[,] dest, int w, int h)
    {
        if (_enableParallel)
        {
            Parallel.For(0, w, (x, loopState) =>
            {
                TransformCol(source, dest, x, h);
            });
        }
        else
        {
            for (int x = 0; x < w; x++)
            {
                TransformCol(source, dest, x, h);
            }
        }
    }

    virtual protected void InverseTransformRows(float[,] source, float[,] dest, int w, int h)
    {
        if (_enableParallel)
        {
            Parallel.For(0, h, (y, loopState) =>
            {
                InverseTransformRow(source, dest, y, w);
            });
        }
        else
        {
            for (int y = 0; y < h; y++)
            {
                InverseTransformRow(source, dest, y, w);
            }
        }
    }

    virtual protected void InverseTransformCols(float[,] source, float[,] dest, int w, int h)
    {
        if (_enableParallel)
        {
            Parallel.For(0, w, (x, loopState) =>
            {
                InverseTransformCol(source, dest, x, h);
            });
        }
        else
        {
            for (int x = 0; x < w; x++)
            {
                InverseTransformCol(source, dest, x, h);
            }
        }
    }

    virtual public void Transform2D(float[,] source)
    {
        lock (_threadSync)
        {
            CheckArrayArgument(source, "source");

            float[,] temp = GetTempArray();
            int w = _width;
            int h = _height;

            while ((w >= _minSize) && (h >= _minSize))
            {
                TransformRows(source, temp, w, h);
                TransformCols(temp, source, w, h);
                w = -(-w >> 1);
                h = -(-h >> 1);
            }
        }
    }

    virtual public void ReverseTransform2D(float[,] source)
    {
        lock (_threadSync)
        {
            CheckArrayArgument(source, "source");

            int log2 = 1;
            int test = 1;

            while (test < (_width | _height))
            {
                test <<= 1;
                log2++;
            }

            float[,] temp = GetTempArray();
            int i = 1;

            while (i <= log2)
            {
                int w = -(-_width >> (log2 - i));
                int h = -(-_height >> (log2 - i));

                if ((w >= _minSize) && (h >= _minSize))
                {
                    InverseTransformCols(source, temp, w, h);
                    InverseTransformRows(temp, source, w, h);
                }
                i++;
            }
        }
    }

    void ModfiyBlock(float[,] source, int n, float[] scaleFactorsMajors, float scaleFactorsMinors, int gridSize, int startX, int startY)
    {
        int endX = startX + gridSize;
        int endY = startY + gridSize;
        if (endX > _width)
            endX = _width;
        if (endY > _height)
            endY = _height;

        bool[,] keep = new bool[gridSize, gridSize];
        float[,] tmpBlock = new float[gridSize, gridSize];

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                float val = source[x, y];
                if (val < 0)
                    val = -val;

                tmpBlock[x - startX, y - startY] = val;
            }
        }
        for (int k = 0; k < n; k++)
        {
            float max = -1.0f;
            int maxIdxX = -1, maxIdxY = -1;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (!keep[x, y])
                        if (tmpBlock[x, y] > max)
                        {
                            max = tmpBlock[x, y];
                            maxIdxX = x;
                            maxIdxY = y;
                        }
                }
            }
            keep[maxIdxX, maxIdxY] = true;

            if (scaleFactorsMajors != null)
            {
                int x = startX + maxIdxX;
                int y = startY + maxIdxY;

                if (x > endX - 1)
                    x = endX - 1;
                if (y > endY - 1)
                    y = endY - 1;
                source[x, y] *= scaleFactorsMajors[k];
            }
        }

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                if (!keep[x - startX, y - startY])
                    source[x, y] *= scaleFactorsMinors;
            }
        }
    }

    void ModifyCoefficients(float[,] source, int n, float[] scaleFactorsMajors, float scaleFactorsMinors, int gridSize)
    {
        CheckArrayArgument(source, "source");
        if (scaleFactorsMajors != null)
        {
            if (scaleFactorsMajors.Length != n)
                throw new ArgumentException("scaleFactorsMajors must be null or the length must be of dimension n (" + n + ")");
        }
        if (gridSize < 1)
            throw new ArgumentException("gridSize (" + gridSize + ") cannot be smaller than 1");
        if (n < 0)
            throw new ArgumentException("n (" + n + ") cannot be negative");
        if (n > gridSize * gridSize)
            throw new ArgumentException("n (" + n + ") cannot be greater than " + gridSize + "*" + gridSize);

        int w = _width / gridSize;
        if ((_width % gridSize) != 0)
            w++;

        int h = _height / gridSize;
        if ((_height % gridSize) != 0)
            h++;

        int numBlocks = w * h;

        if (_enableParallel)
        {
            Parallel.For(0, numBlocks, (block, loopState) =>
            {
                int startX = (block % w) * gridSize;
                int startY = (block / w) * gridSize;
                ModfiyBlock(source, n, scaleFactorsMajors, scaleFactorsMinors, gridSize, startX, startY);
            });
        }
        else
        {
            for (int block = 0; block < numBlocks; block++)
            {
                int startX = (block % w) * gridSize;
                int startY = (block / w) * gridSize;
                ModfiyBlock(source, n, scaleFactorsMajors, scaleFactorsMinors, gridSize, startX, startY);
            }
        }
    }

    virtual public void ScaleCoefficients(float[,] source, float[] scaleFactors, int gridSize)
    {
        lock (_threadSync)
        {
            if (scaleFactors == null)
                throw new ArgumentException("scaleFactors cannot be null");
            if (scaleFactors.Length > gridSize * gridSize)
                throw new ArgumentException("scaleFactors length can't be greater than " + gridSize * gridSize);

            ModifyCoefficients(source, scaleFactors.Length, scaleFactors, 1.0f, gridSize);
        }
    }
}