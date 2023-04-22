using System;
using System.Collections.Generic;

namespace SOTFEdit.Infrastructure.Converters;

public static class CoordinateConverter
{
    private static readonly float IngameToPixelOffsetX;
    private static readonly float IngameToPixelOffsetY;
    private static readonly float IngameToPixelScaleX;
    private static readonly float IngameToPixelScaleY;

    static CoordinateConverter()
    {
        // Input data: pixel coordinates and corresponding in-game coordinates
        float[] pixelXs = { 1311, 1592.5f, 1426, 1037, 688, 772.3f, 709.0f, 786.2f, 789.8f, 899.5f, 1259.8f };
        float[] pixelYs = { 2064, 2155, 1494.5f, 1425, 1407, 1486.3f, 1713.7f, 1867.3f, 2020.1f, 2125.4f, 1822.8f };
        float[] ingameXs =
        {
            -719.0499f, -444.96793f, -604.09875f, -989.2298f, -1328.8882f, -1245.4705f, -1308.0143f, -1232.5912f,
            -1226.8536f,
            -1112.021f, -770.1515f
        };
        float[] ingameYs =
        {
            -17.139717f, -103.88483f, 541.12946f, 611.64136f, 625.7892f, 549.34155f, 326.0083f, 176.03516f, 27.191362f,
            -74.49177f, 219.75914f
        };

        // Calculate the inverse scale and offset for the x-axis
        var inverseScaleXandOffsetX = CalculateScaleAndOffset(ingameXs, pixelXs);
        IngameToPixelScaleX = inverseScaleXandOffsetX.Item1;
        IngameToPixelOffsetX = inverseScaleXandOffsetX.Item2;

        // Calculate the inverse scale and offset for the y-axis
        var inverseScaleYandOffsetY = CalculateScaleAndOffset(ingameYs, pixelYs);
        IngameToPixelScaleY = inverseScaleYandOffsetY.Item1;
        IngameToPixelOffsetY = inverseScaleYandOffsetY.Item2;
    }

    private static Tuple<float, float> CalculateScaleAndOffset(IReadOnlyList<float> fromValues,
        IReadOnlyList<float> toValues)
    {
        var n = fromValues.Count;
        float sumX = 0, sumY = 0, sumXy = 0, sumXx = 0;

        for (var i = 0; i < n; i++)
        {
            sumX += fromValues[i];
            sumY += toValues[i];
            sumXy += fromValues[i] * toValues[i];
            sumXx += fromValues[i] * fromValues[i];
        }

        var scale = (n * sumXy - sumX * sumY) / (n * sumXx - sumX * sumX);
        var offset = (sumY - scale * sumX) / n;

        return new Tuple<float, float>(scale, offset);
    }

    public static (float, float) IngameToPixel(float ingameX, float ingameY)
    {
        var pixelX = IngameToPixelScaleX * ingameX + IngameToPixelOffsetX;
        var pixelY = IngameToPixelScaleY * ingameY + IngameToPixelOffsetY;
        return (pixelX, pixelY);
    }
}