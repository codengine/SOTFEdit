using System;
using System.Collections.Generic;

namespace SOTFEdit.Infrastructure.Converters;

public class CoordinateConverter
{
    private readonly double _ingameToPixelOffsetX;
    private readonly double _ingameToPixelOffsetY;
    private readonly double _ingameToPixelScaleX;
    private readonly double _ingameToPixelScaleY;
    private readonly double _pixelToIngameOffsetX;
    private readonly double _pixelToIngameOffsetY;
    private readonly double _pixelToIngameScaleX;
    private readonly double _pixelToIngameScaleY;

    public CoordinateConverter()
    {
        // Input data: pixel coordinates and corresponding in-game coordinates
        double[] pixelXs = { 1311, 1592.5, 1426, 1037, 688, 772.3, 709.0, 786.2, 789.8, 899.5, 1259.8 };
        double[] pixelYs = { 2064, 2155, 1494.5, 1425, 1407, 1486.3, 1713.7, 1867.3, 2020.1, 2125.4, 1822.8 };
        double[] ingameXs =
        {
            -719.0499, -444.96793, -604.09875, -989.2298, -1328.8882, -1245.4705, -1308.0143, -1232.5912, -1226.8536,
            -1112.021, -770.1515
        };
        double[] ingameYs =
        {
            -17.139717, -103.88483, 541.12946, 611.64136, 625.7892, 549.34155, 326.0083, 176.03516, 27.191362,
            -74.49177, 219.75914
        };

        // Calculate the scale and offset for the x-axis
        var scaleXandOffsetX = CalculateScaleAndOffset(pixelXs, ingameXs);
        _pixelToIngameScaleX = scaleXandOffsetX.Item1;
        _pixelToIngameOffsetX = scaleXandOffsetX.Item2;

        // Calculate the scale and offset for the y-axis
        var scaleYandOffsetY = CalculateScaleAndOffset(pixelYs, ingameYs);
        _pixelToIngameScaleY = scaleYandOffsetY.Item1;
        _pixelToIngameOffsetY = scaleYandOffsetY.Item2;

        // Calculate the inverse scale and offset for the x-axis
        var inverseScaleXandOffsetX = CalculateScaleAndOffset(ingameXs, pixelXs);
        _ingameToPixelScaleX = inverseScaleXandOffsetX.Item1;
        _ingameToPixelOffsetX = inverseScaleXandOffsetX.Item2;

        // Calculate the inverse scale and offset for the y-axis
        var inverseScaleYandOffsetY = CalculateScaleAndOffset(ingameYs, pixelYs);
        _ingameToPixelScaleY = inverseScaleYandOffsetY.Item1;
        _ingameToPixelOffsetY = inverseScaleYandOffsetY.Item2;
    }

    private static Tuple<double, double> CalculateScaleAndOffset(IReadOnlyList<double> fromValues,
        IReadOnlyList<double> toValues)
    {
        var n = fromValues.Count;
        double sumX = 0, sumY = 0, sumXy = 0, sumXx = 0;

        for (var i = 0; i < n; i++)
        {
            sumX += fromValues[i];
            sumY += toValues[i];
            sumXy += fromValues[i] * toValues[i];
            sumXx += fromValues[i] * fromValues[i];
        }

        var scale = (n * sumXy - sumX * sumY) / (n * sumXx - sumX * sumX);
        var offset = (sumY - scale * sumX) / n;

        return new Tuple<double, double>(scale, offset);
    }

    public (double, double) PixelToIngame(double pixelX, double pixelY)
    {
        var ingameX = _pixelToIngameScaleX * pixelX + _pixelToIngameOffsetX;
        var ingameY = _pixelToIngameScaleY * pixelY + _pixelToIngameOffsetY;
        return (ingameX, ingameY);
    }

    public (double, double) IngameToPixel(double ingameX, double ingameY)
    {
        var pixelX = _ingameToPixelScaleX * ingameX + _ingameToPixelOffsetX;
        var pixelY = _ingameToPixelScaleY * ingameY + _ingameToPixelOffsetY;
        return (pixelX, pixelY);
    }
}