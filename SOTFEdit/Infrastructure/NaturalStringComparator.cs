using System.Collections.Generic;

namespace SOTFEdit.Infrastructure;

/// <summary>
///     String comparer that applies a “natural sort” algorithm.
/// </summary>
/// <remarks>
///     Original Java code by Stanislav Bokach (https://stackoverflow.com/a/58249974)
/// </remarks>
public sealed class NaturalStringComparator : IComparer<string>
{
    /// <summary>
    ///     Compares two strings and returns a value indicating whether one is less than,
    ///     equal to, or greater than the other, according to a “natural sort” algorithm.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />.
    ///     Less than zero: <paramref name="x" /> is less than <paramref name="y" />.
    ///     Zero: <paramref name="x" /> equals <paramref name="y" />.
    ///     Greater than zero: <paramref name="x" /> is greater than <paramref name="y" />.
    /// </returns>
    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var indexX = 0;
        var indexY = 0;
        while (true)
        {
            // Handle the case when one string has ended.
            if (indexX == x.Length)
            {
                return indexY == y.Length ? 0 : -1;
            }

            if (indexY == y.Length)
            {
                return 1;
            }

            var charX = x[indexX];
            var charY = y[indexY];
            if (char.IsDigit(charX) && char.IsDigit(charY))
            {
                // Skip leading zeroes in numbers.
                while (indexX < x.Length && x[indexX] == '0')
                {
                    indexX++;
                }

                while (indexY < y.Length && y[indexY] == '0')
                {
                    indexY++;
                }

                // Find the end of numbers
                var endNumberX = indexX;
                var endNumberY = indexY;
                while (endNumberX < x.Length && char.IsDigit(x[endNumberX]))
                {
                    endNumberX++;
                }

                while (endNumberY < y.Length && char.IsDigit(y[endNumberY]))
                {
                    endNumberY++;
                }

                var digitsLengthX = endNumberX - indexX;
                var digitsLengthY = endNumberY - indexY;

                // If the lengths are different, then the longer number is bigger
                if (digitsLengthX != digitsLengthY)
                {
                    return digitsLengthX - digitsLengthY;
                }

                // Compare numbers digit by digit
                while (indexX < endNumberX)
                {
                    if (x[indexX] != y[indexY])
                    {
                        return x[indexX] - y[indexY];
                    }

                    indexX++;
                    indexY++;
                }
            }
            else
            {
                // Plain characters comparison
                var compareResult = char.ToUpperInvariant(charX).CompareTo(char.ToUpperInvariant(charY));
                if (compareResult != 0)
                {
                    return compareResult;
                }

                indexX++;
                indexY++;
            }
        }
    }
}